using Xunit;
using CLDV6212_CoffeeNChill_Functions.Models;
using CLDV6212_CoffeeNChill_Functions.Services;
using CLDV6212_CoffeeNChill_Functions.Functions;

namespace CoffeeNChill.Tests
{
    public class MenuItemEntityTests
    {
        [Fact]
        public void MenuItemEntity_ShouldStoreNameAndPrice()
        {
            var item = new MenuItemEntity
            {
                Name = "Cappuccino",
                Price = 35.00
            };

            Assert.Equal("Cappuccino", item.Name);
            Assert.Equal(35.00, item.Price);
        }

        [Fact]
        public void MenuItemEntity_PartitionKeyAndRowKey_CanBeSet()
        {
            var item = new MenuItemEntity
            {
                PartitionKey = "Drinks",
                RowKey = "DRK-001"
            };

            Assert.Equal("Drinks", item.PartitionKey);
            Assert.Equal("DRK-001", item.RowKey);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(25.5)]
        [InlineData(99)]
        public void MenuItemEntity_Price_ShouldBePositive(double price)
        {
            var item = new MenuItemEntity { Price = price };
            Assert.True(item.Price > 0);
        }
    }
}
