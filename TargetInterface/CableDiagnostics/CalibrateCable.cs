namespace TargetInterface.CableDiagnostics
{
    public class CalibrateCable
    {
        private float nvp;
        public float NVP
        {
            get
            {
                return this.nvp;
            }

            set
            {
                if (this.nvp > 1.0f || this.nvp < 0.0f)
                {
                    this.nvp = 0.0f;
                }
                else
                {
                    this.nvp = value;
                }
            }
        }
        public float Coeff0 { get; set; }
        public float Coeffi { get; set; }
        public string FileName { get; set; }
    }
}
