using HireMe.Models;

namespace HireMe.SeedingData
{
    public static class DefaultGovernorates
    {
        // البداية: القاهرة والإسكندرية
        private static readonly Governorate Cairo = new Governorate { Id = 1, NameEnglish = "Cairo", NameArabic = "القاهرة" };
        private static readonly Governorate Alexandria = new Governorate { Id = 2, NameEnglish = "Alexandria", NameArabic = "الإسكندرية" };

        // شمال ووسط مصر
        private static readonly Governorate Matrouh = new Governorate { Id = 3, NameEnglish = "Matrouh", NameArabic = "مرسى مطروح" };
        private static readonly Governorate Beheira = new Governorate { Id = 4, NameEnglish = "Beheira", NameArabic = "البحيرة" };
        private static readonly Governorate KafrElSheikh = new Governorate { Id = 5, NameEnglish = "Kafr El Sheikh", NameArabic = "كفر الشيخ" };
        private static readonly Governorate Dakahlia = new Governorate { Id = 6, NameEnglish = "Dakahlia", NameArabic = "الدقهلية" };
        private static readonly Governorate Damietta = new Governorate { Id = 7, NameEnglish = "Damietta", NameArabic = "دمياط" };
        private static readonly Governorate Gharbia = new Governorate { Id = 8, NameEnglish = "Gharbia", NameArabic = "الغربية" };
        private static readonly Governorate Monufia = new Governorate { Id = 9, NameEnglish = "Monufia", NameArabic = "المنوفية" };
        private static readonly Governorate Qalyubia = new Governorate { Id = 10, NameEnglish = "Qalyubia", NameArabic = "القليوبية" };
        private static readonly Governorate PortSaid = new Governorate { Id = 11, NameEnglish = "Port Said", NameArabic = "بورسعيد" };
        private static readonly Governorate Ismailia = new Governorate { Id = 12, NameEnglish = "Ismailia", NameArabic = "الإسماعيلية" };
        private static readonly Governorate Suez = new Governorate { Id = 13, NameEnglish = "Suez", NameArabic = "السويس" };
        private static readonly Governorate Sharqia = new Governorate { Id = 14, NameEnglish = "Sharqia", NameArabic = "الشرقية" };
        private static readonly Governorate Fayoum = new Governorate { Id = 15, NameEnglish = "Fayoum", NameArabic = "الفيوم" };
        private static readonly Governorate Giza = new Governorate { Id = 16, NameEnglish = "Giza", NameArabic = "الجيزة" };

        // المحافظات الصحراوية
        private static readonly Governorate NewValley = new Governorate { Id = 17, NameEnglish = "New Valley", NameArabic = "الوادي الجديد" };
        private static readonly Governorate NorthSinai = new Governorate { Id = 18, NameEnglish = "North Sinai", NameArabic = "شمال سيناء" };
        private static readonly Governorate SouthSinai = new Governorate { Id = 19, NameEnglish = "South Sinai", NameArabic = "جنوب سيناء" };
        private static readonly Governorate RedSea = new Governorate { Id = 20, NameEnglish = "Red Sea", NameArabic = "البحر الأحمر" };

        // جنوب مصر بالترتيب من الشمال للجنوب
        private static readonly Governorate BeniSuef = new Governorate { Id = 21, NameEnglish = "Beni Suef", NameArabic = "بني سويف" };
        private static readonly Governorate Minya = new Governorate { Id = 22, NameEnglish = "Minya", NameArabic = "المنيا" };
        private static readonly Governorate Assiut = new Governorate { Id = 23, NameEnglish = "Assiut", NameArabic = "أسيوط" };
        private static readonly Governorate Sohag = new Governorate { Id = 24, NameEnglish = "Sohag", NameArabic = "سوهاج" };
        private static readonly Governorate Qena = new Governorate { Id = 25, NameEnglish = "Qena", NameArabic = "قنا" };
        private static readonly Governorate Luxor = new Governorate { Id = 26, NameEnglish = "Luxor", NameArabic = "الأقصر" };
        private static readonly Governorate Aswan = new Governorate { Id = 27, NameEnglish = "Aswan", NameArabic = "أسوان" };


        public static readonly List<Governorate> All = new()
        {
            Cairo, Alexandria,
            Matrouh, Beheira, KafrElSheikh, Dakahlia, Damietta, Gharbia, Monufia, Qalyubia,
            PortSaid, Ismailia, Suez, Sharqia, Fayoum, Giza,
            NewValley, NorthSinai, SouthSinai, RedSea,
            BeniSuef, Minya, Assiut, Sohag, Qena, Luxor, Aswan
        };
    }
}
