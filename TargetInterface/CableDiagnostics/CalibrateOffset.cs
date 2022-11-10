namespace TargetInterface.CableDiagnostics
{
    public class CalibrateOffset
    {
        /// <summary>
        /// Offset.
        /// </summary>
        private float offset;
        /// <summary>
        /// Gets or sets the offset.
        /// </summary>
        public float Offset
        {
            get
            {
                return this.offset;
            }

            set
            {
                if (value >= 0.0f && value <= 1000.0f && this.offset != value)
                {
                    this.offset = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string FileName { get; set; }
    }
}
