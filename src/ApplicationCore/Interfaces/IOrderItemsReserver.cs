﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

namespace Microsoft.eShopWeb.ApplicationCore.Interfaces;
public interface IOrderItemsReserver
{
    Task ReserveAsync(IEnumerable<OrderItem> orderItems);
}
