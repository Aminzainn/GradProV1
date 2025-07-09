namespace GP.Models
{
    public class PlaceType
    {
        public int Id { get; set; }
        public string Name { get; set; } // Hall, Stadium, Café

        public ICollection<Place> Places { get; set; }
    }
}
