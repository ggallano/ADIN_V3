namespace TargetInterface.CableDiagnostics
{
    public class CalibrateCable
    {
        /// <summary>
        /// Nvp storage.
        /// </summary>
        private float nvp;
        /// <summary>
        /// Gets or sets the Nvp.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the Coeff0.
        /// </summary>
        public float Coeff0 { get; set; }

        /// <summary>
        /// Gets or sets the Coeff1.
        /// </summary>
        public float Coeffi { get; set; }

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string FileName { get; set; }
    }
}
