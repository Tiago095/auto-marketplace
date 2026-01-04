using System.Collections.Generic;


namespace AutoMatch.Models.ViewModels
{
    public class ProfileViewModel
    {
        public int Id_User { get; set; }

        // From Utilizadores
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ProfileImageUrl { get; set; }

        public bool IsSeller { get; set; }
        public bool IsBuyer { get; set; }

        // From Compradores
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string Phone { get; set; }

        public List<CarOrderViewModel> Orders { get; set; } = new();
        public List<CarListingViewModel> Listings { get; set; } = new();
    }



    public class CarOrderViewModel
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
    }


    public class CarListingViewModel
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string CreatedAt { get; set; }
        public string State { get; set; }
    }

    public class FeaturedCarViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public int Preco { get; set; }
        public string ImageUrl { get; set; }
    }
}