using System;

namespace xBRZ.NET
{
    //http://intrepidis.blogspot.com/2014/02/xbrz-in-java.html
    /*
        -------------------------------------------------------------------------
        | xBRZ: "Scale by rules" - high quality image upscaling filter by Zenju |
        -------------------------------------------------------------------------
        using a modified approach of xBR:
        http://board.byuu.org/viewtopic.php?f=10&t=2248
        - new rule set preserving small image features
        - support multithreading
        - support 64 bit architectures
        - support processing image slices
    */

    /*
        -> map source (srcWidth * srcHeight) to target (scale * width x scale * height)
        image, optionally processing a half-open slice of rows [yFirst, yLast) only
        -> color format: ARGB (BGRA char order), alpha channel unused
        -> support for source/target pitch in chars!
        -> if your emulator changes only a few image slices during each cycle
        (e.g. Dosbox) then there's no need to run xBRZ on the complete image:
        Just make sure you enlarge the source image slice by 2 rows on top and
        2 on bottom (this is the additional range the xBRZ algorithm is using
        during analysis)
        Caveat: If there are multiple changed slices, make sure they do not overlap
        after adding these additional rows in order to avoid a memory race condition 
        if you are using multiple threads for processing each enlarged slice!

        THREAD-SAFETY: - parts of the same image may be scaled by multiple threads
        as long as the [yFirst, yLast) ranges do not overlap!
        - there is a minor inefficiency for the first row of a slice, so avoid
        processing single rows only
        */

    /*
        Converted to Java 7 by intrepidis. It would have been nice to use
        Java 8 lambdas, but Java 7 is more ubiquitous at the time of writing,
        so this code uses anonymous local classes instead.
        Regarding multithreading, each thread should have its own instance
        of the xBRZ class.
    */

    public class xBrzConv
    {
        public void ScaleImage(ScaleSize scaleSize, int[] src, int[] trg, int srcWidth, int srcHeight, ScalerCfg cfg, int yFirst, int yLast)
        {
            _scaleSize = scaleSize;
            _cfg = cfg;
            _colorDistance = new ColorDist(_cfg);
            _colorEqualizer = new ColorEq(_cfg);
            ScaleImageImpl(src, trg, srcWidth, srcHeight, yFirst, yLast);
        }

        private ScalerCfg _cfg;
        private ScaleSize _scaleSize;
        private OutputMatrix _outputMatrix;
        private readonly BlendResult _blendResult = new BlendResult();

        private ColorDist _colorDistance;
        private ColorEq _colorEqualizer;

        //fill block with the given color
        private static void FillBlock(int[] trg, int trgi, int pitch, int col, int blockSize)
        {
            for (var y = 0; y < blockSize; ++y, trgi += pitch)
            {
                for (var x = 0; x < blockSize; ++x)
                {
                    trg[trgi + x] = col;
                }
            }
        }

        //detect blend direction
        private void PreProcessCorners( Kernel_4x4 ker)
        {
            _blendResult.Reset();

            if ((ker.f == ker.g && ker.j == ker.k) || (ker.f == ker.j && ker.g == ker.k)) return;

            var dist = _colorDistance;
            
            var weight = 4;
            var jg = dist.DistYCbCr(ker.i, ker.f) + dist.DistYCbCr(ker.f, ker.c) + dist.DistYCbCr(ker.n, ker.k) + dist.DistYCbCr(ker.k, ker.h) + weight * dist.DistYCbCr(ker.j, ker.g);
            var fk = dist.DistYCbCr(ker.e, ker.j) + dist.DistYCbCr(ker.j, ker.o) + dist.DistYCbCr(ker.b, ker.g) + dist.DistYCbCr(ker.g, ker.l) + weight * dist.DistYCbCr(ker.f, ker.k);

            if (jg < fk)
            {
                var dominantGradient = _cfg.DominantDirectionThreshold * jg < fk;
                if (ker.f != ker.g && ker.f != ker.j)
                {
                    _blendResult.f = (char)(dominantGradient ? BlendType.BLEND_DOMINANT : BlendType.BLEND_NORMAL);
                }
                if (ker.k != ker.j && ker.k != ker.g)
                {
                    _blendResult.k = (char)(dominantGradient ? BlendType.BLEND_DOMINANT : BlendType.BLEND_NORMAL);
                }
            }
            else if (fk < jg)
            {
                var dominantGradient = _cfg.DominantDirectionThreshold * fk < jg;
                if (ker.j != ker.f && ker.j != ker.k)
                {
                    _blendResult.j = (char)(dominantGradient ? BlendType.BLEND_DOMINANT : BlendType.BLEND_NORMAL);
                }
                if (ker.g != ker.f && ker.g != ker.k)
                {
                    _blendResult.g = (char)(dominantGradient ? BlendType.BLEND_DOMINANT : BlendType.BLEND_NORMAL);
                }
            }
        }

        /*
            input kernel area naming convention:
            -------------
            | A | B | C |
            ----|---|---|
            | D | E | F | //input pixel is at position E
            ----|---|---|
            | G | H | I |
            -------------
            blendInfo: result of preprocessing all four corners of pixel "e"
        */
        private void ScalePixel(IScaler scaler, int rotDeg, Kernel_3x3 ker, int[] trg, int trgi, int trgWidth, char blendInfo)
        {
            // int a = ker._[Rot._[(0 << 2) + rotDeg]];
            var b = ker._[Rot._[(1 << 2) + rotDeg]];
            var c = ker._[Rot._[(2 << 2) + rotDeg]];
            var d = ker._[Rot._[(3 << 2) + rotDeg]];
            var e = ker._[Rot._[(4 << 2) + rotDeg]];
            var f = ker._[Rot._[(5 << 2) + rotDeg]];
            var g = ker._[Rot._[(6 << 2) + rotDeg]];
            var h = ker._[Rot._[(7 << 2) + rotDeg]];
            var i = ker._[Rot._[(8 << 2) + rotDeg]];

            var blend = BlendInfo.Rotate(blendInfo, (RotationDegree)rotDeg);

            if ((BlendType)BlendInfo.GetBottomR(blend) == BlendType.BLEND_NONE) return;

            var eq = _colorEqualizer;
            var dist = _colorDistance;

            bool doLineBlend;

            if (BlendInfo.GetBottomR(blend) >= (char)BlendType.BLEND_DOMINANT)
            {
                doLineBlend = true;
            }
            //make sure there is no second blending in an adjacent
            //rotation for this pixel: handles insular pixels, mario eyes
            //but support double-blending for 90� corners
            else if (BlendInfo.GetTopR(blend) != (char)BlendType.BLEND_NONE && !eq.IsColorEqual(e, g))
            {
                doLineBlend = false;
            }
            else if (BlendInfo.GetBottomL(blend) != (char)BlendType.BLEND_NONE && !eq.IsColorEqual(e, c))
            {
                doLineBlend = false;
            }
            //no full blending for L-shapes; blend corner only (handles "mario mushroom eyes")
            else if (eq.IsColorEqual(g, h) && eq.IsColorEqual(h, i) && eq.IsColorEqual(i, f) && eq.IsColorEqual(f, c) && !eq.IsColorEqual(e, i))
            {
                doLineBlend = false;
            }
            else
            {
                doLineBlend = true;
            }

            //choose most similar color
            var px = dist.DistYCbCr(e, f) <= dist.DistYCbCr(e, h) ? f : h;

            var out_ = _outputMatrix;
            out_.Move(rotDeg, trgi);

            if (!doLineBlend)
            {
                scaler.BlendCorner(px, out_);
                return;
            }

            //test sample: 70% of values max(fg, hc) / min(fg, hc)
            //are between 1.1 and 3.7 with median being 1.9
             var fg = dist.DistYCbCr(f, g);
             var hc = dist.DistYCbCr(h, c);

             var haveShallowLine = _cfg.SteepDirectionThreshold * fg <= hc && e != g && d != g;
             var haveSteepLine = _cfg.SteepDirectionThreshold * hc <= fg && e != c && b != c;

            if (haveShallowLine)
            {
                if (haveSteepLine)
                {
                    scaler.BlendLineSteepAndShallow(px, out_);
                }
                else
                {
                    scaler.BlendLineShallow(px, out_);
                } 
            }
            else
            {
                if (haveSteepLine)
                {
                    scaler.BlendLineSteep(px, out_);
                }
                else
                {
                    scaler.BlendLineDiagonal(px, out_);
                }
            }
        }

        //scaler policy: see "Scaler2x" reference implementation
        private void ScaleImageImpl(int[] src, int[] trg, int srcWidth, int srcHeight, int yFirst, int yLast)
        {
            yFirst = Math.Max(yFirst, 0);
            yLast = Math.Min(yLast, srcHeight);

            if (yFirst >= yLast || srcWidth <= 0) return;

            var trgWidth = srcWidth * _scaleSize.Size;

            //temporary buffer for "on the fly preprocessing"
            var preProcBuffer = new char[srcWidth];

            var ker4 = new Kernel_4x4();

            //initialize preprocessing buffer for first row:
            //detect upper left and right corner blending
            //this cannot be optimized for adjacent processing
            //stripes; we must not allow for a memory race condition!
            if (yFirst > 0)
            {
                var y = yFirst - 1;

                var s_m1 = srcWidth * Math.Max(y - 1, 0);
                var s_0 = srcWidth * y; //center line
                var s_p1 = srcWidth * Math.Min(y + 1, srcHeight - 1);
                var s_p2 = srcWidth * Math.Min(y + 2, srcHeight - 1);

                for (var x = 0; x<srcWidth; ++x)
                {
                    var x_m1 = Math.Max(x - 1, 0);
                    var x_p1 = Math.Min(x + 1, srcWidth - 1);
                    var x_p2 = Math.Min(x + 2, srcWidth - 1);

                    //read sequentially from memory as far as possible
                    ker4.a = src[s_m1 + x_m1];
                    ker4.b = src[s_m1 + x];
                    ker4.c = src[s_m1 + x_p1];
                    ker4.d = src[s_m1 + x_p2];

                    ker4.e = src[s_0 + x_m1];
                    ker4.f = src[s_0 + x];
                    ker4.g = src[s_0 + x_p1];
                    ker4.h = src[s_0 + x_p2];

                    ker4.i = src[s_p1 + x_m1];
                    ker4.j = src[s_p1 + x];
                    ker4.k = src[s_p1 + x_p1];
                    ker4.l = src[s_p1 + x_p2];

                    ker4.m = src[s_p2 + x_m1];
                    ker4.n = src[s_p2 + x];
                    ker4.o = src[s_p2 + x_p1];
                    ker4.p = src[s_p2 + x_p2];

                    PreProcessCorners(ker4); // writes to blendResult
                    /*
                    preprocessing blend result:
                    ---------
                    | F | G | //evalute corner between F, G, J, K
                    ----|---| //input pixel is at position F
                    | J | K |
                    ---------
                    */
                    preProcBuffer[x] = BlendInfo.SetTopR(preProcBuffer[x], _blendResult.j);

                    if (x + 1 < srcWidth)
                    {
                        preProcBuffer[x + 1] = BlendInfo.SetTopL(preProcBuffer[x + 1], _blendResult.k);
                    }
                }
            }

            _outputMatrix = new OutputMatrix(_scaleSize.Size, trg, trgWidth);

            var blend_xy = (char)0;
            var blend_xy1 = (char)0;

            var ker3 = new Kernel_3x3();

            for (var y = yFirst; y<yLast; ++y)
            {
                //consider MT "striped" access
                var trgi = _scaleSize.Size * y * trgWidth;

                var s_m1 = srcWidth * Math.Max(y - 1, 0);
                var s_0 = srcWidth * y; //center line
                var s_p1 = srcWidth * Math.Min(y + 1, srcHeight - 1);
                var s_p2 = srcWidth * Math.Min(y + 2, srcHeight - 1);

                blend_xy1 = (char)0; //corner blending for current (x, y + 1) position

                for (var x = 0; x < srcWidth; ++x, trgi += _scaleSize.Size)
                {
                    var x_m1 = Math.Max(x - 1, 0);
                    var x_p1 = Math.Min(x + 1, srcWidth - 1);
                    var x_p2 = Math.Min(x + 2, srcWidth - 1);

                    //evaluate the four corners on bottom-right of current pixel
                    //blend_xy for current (x, y) position
                    
                    //read sequentially from memory as far as possible
                    ker4.a = src[s_m1 + x_m1];
                    ker4.b = src[s_m1 + x];
                    ker4.c = src[s_m1 + x_p1];
                    ker4.d = src[s_m1 + x_p2];

                    ker4.e = src[s_0 + x_m1];
                    ker4.f = src[s_0 + x];
                    ker4.g = src[s_0 + x_p1];
                    ker4.h = src[s_0 + x_p2];

                    ker4.i = src[s_p1 + x_m1];
                    ker4.j = src[s_p1 + x];
                    ker4.k = src[s_p1 + x_p1];
                    ker4.l = src[s_p1 + x_p2];

                    ker4.m = src[s_p2 + x_m1];
                    ker4.n = src[s_p2 + x];
                    ker4.o = src[s_p2 + x_p1];
                    ker4.p = src[s_p2 + x_p2];

                    PreProcessCorners(ker4); // writes to blendResult

                    /*
                        preprocessing blend result:
                        ---------
                        | F | G | //evaluate corner between F, G, J, K
                        ----|---| //current input pixel is at position F
                        | J | K |
                        ---------
                    */

                    //all four corners of (x, y) have been determined at
                    //this point due to processing sequence!
                    blend_xy = BlendInfo.SetBottomR(preProcBuffer[x], _blendResult.f);

                    //set 2nd known corner for (x, y + 1)
                    blend_xy1 = BlendInfo.SetTopR(blend_xy1, _blendResult.j);
                    //store on current buffer position for use on next row
                    preProcBuffer[x] = blend_xy1;

                    //set 1st known corner for (x + 1, y + 1) and
                    //buffer for use on next column
                    blend_xy1 = BlendInfo.SetTopL((char)0, _blendResult.k);

                    if (x + 1 < srcWidth)
                    {
                        //set 3rd known corner for (x + 1, y)
                        preProcBuffer[x + 1] = BlendInfo.SetBottomL(preProcBuffer[x + 1], _blendResult.g);
                    }

                    //fill block of size scale * scale with the given color
                    //  //place *after* preprocessing step, to not overwrite the
                    //  //results while processing the the last pixel!
                    FillBlock(trg, trgi, trgWidth, src[s_0 + x], _scaleSize.Size);

                    //blend four corners of current pixel
                    if (blend_xy == 0) continue;

                    const int a = 0, b = 1, c = 2, d = 3, e = 4, f = 5, g = 6, h = 7, i = 8;

                    //read sequentially from memory as far as possible
                    ker3._[a] = src[s_m1 + x_m1];
                    ker3._[b] = src[s_m1 + x];
                    ker3._[c] = src[s_m1 + x_p1];

                    ker3._[d] = src[s_0 + x_m1];
                    ker3._[e] = src[s_0 + x];
                    ker3._[f] = src[s_0 + x_p1];

                    ker3._[g] = src[s_p1 + x_m1];
                    ker3._[h] = src[s_p1 + x];
                    ker3._[i] = src[s_p1 + x_p1];

                    ScalePixel(_scaleSize.Scaler, (int)RotationDegree.ROT_0, ker3, trg, trgi, trgWidth, blend_xy);
                    ScalePixel(_scaleSize.Scaler, (int)RotationDegree.ROT_90, ker3, trg, trgi, trgWidth, blend_xy);
                    ScalePixel(_scaleSize.Scaler, (int)RotationDegree.ROT_180, ker3, trg, trgi, trgWidth, blend_xy);
                    ScalePixel(_scaleSize.Scaler, (int)RotationDegree.ROT_270, ker3, trg, trgi, trgWidth, blend_xy);
               }
            }
        }
    }
}
