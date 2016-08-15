namespace xBRZ.NET
{
    internal enum BlendType
    {
        // These blend types must fit into 2 bits.
        BLEND_NONE = 0, //do not blend
        BLEND_NORMAL = 1,//a normal indication to blend
        BLEND_DOMINANT = 2 //a strong indication to blend
    }
}
