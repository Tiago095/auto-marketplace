namespace AutoMatch.Models.ViewModels
{
    public class EditProfileViewModel
    {
        public string UserName { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string SelectedLocalidade { get; set; } // Address
        public string SelectedCodigoPostal { get; set; }
    }
}
