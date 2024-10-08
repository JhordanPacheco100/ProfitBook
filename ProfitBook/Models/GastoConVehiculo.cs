namespace ProfitBook.Models
{
    public class GastoConVehiculo
    {
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public int KmFinal { get; set; }
        public int KmInicial { get; set; }
        public int KilometrajeRecorrido { get; set; }
    }
}
