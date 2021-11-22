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
                if (value >= 0.0f && value <= 1.0f)
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
