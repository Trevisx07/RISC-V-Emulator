namespace RiscVEmulator.Models
{
    public class EmulatorViewModel
    {
        public ProgramRecord Program { get; set; }
        public uint[] Registers { get; set; }
        public byte[] Memory { get; set; }
        public uint Pc { get; set; }
        public string ErrorMessage { get; set; } // Added for error handling
    }
}