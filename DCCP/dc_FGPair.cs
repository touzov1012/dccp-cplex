
namespace DCCP
{
    public struct dc_FGPair
    {
        public dc_Func f;
        public dc_Func g;

        public dc_FGPair(dc_Func f, dc_Func g)
        {
            this.f = f;
            this.g = g;
        }
    }
}
