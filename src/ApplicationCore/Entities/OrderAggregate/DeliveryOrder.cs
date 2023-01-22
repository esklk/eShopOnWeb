using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
public class DeliveryOrder
{
    public Address? ShippingAddress { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = Array.Empty<OrderItem>();
}
