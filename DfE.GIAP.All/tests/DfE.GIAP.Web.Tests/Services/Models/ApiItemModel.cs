namespace DfE.GIAP.Service.Tests.Models
{
    public class ApiItemModel
    {
        public int Id { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is ApiItemModel other)
            {
                return this.Id.Equals(other.Id);
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
