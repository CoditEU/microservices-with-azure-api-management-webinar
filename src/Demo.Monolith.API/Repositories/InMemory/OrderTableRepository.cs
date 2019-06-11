﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.Monolith.API.Contracts.v1;
using Demo.Monolith.API.Data.Contracts.v1;
using Demo.Monolith.API.Data.Providers;
using Demo.Monolith.API.Repositories.Interfaces;
using Newtonsoft.Json;

namespace Demo.Monolith.API.Repositories.InMemory
{
    public class OrderTableRepository : IOrderRepository
    {
        private readonly TableStorageAccessor _tableStorageAccessor;
        private const string TableName = "orders";

        public OrderTableRepository(TableStorageAccessor tableStorageAccessor)
        {
            _tableStorageAccessor = tableStorageAccessor;
        }

        public async Task<string> CreateAsync(Order createdOrder)
        {
            var confirmationId = Guid.NewGuid().ToString();

            var orderTableEntity = new OrderTableEntity
            {
                PartitionKey = confirmationId,
                RowKey = confirmationId,
                ConfirmationId = confirmationId,
                CustomerFirstName = createdOrder.Customer.FirstName,
                CustomerLastName = createdOrder.Customer.LastName,
                Basket = JsonConvert.SerializeObject(createdOrder.Basket),
                CustomerAddress = JsonConvert.SerializeObject(createdOrder.Customer.Address)
            };

            await _tableStorageAccessor.PersistAsync(TableName, orderTableEntity);

            return confirmationId;
        }

        public async Task<Order> GetAsync(string confirmationId)
        {
            var persistedOrder = await _tableStorageAccessor.GetAsync<OrderTableEntity>(TableName, confirmationId, confirmationId);

            Order order = null;
            if (persistedOrder != null)
            {
                order = new Order
                {
                    Basket = JsonConvert.DeserializeObject<List<OrderLine>>(persistedOrder.Basket),
                    Customer = new Customer
                    {
                        FirstName = persistedOrder.CustomerFirstName,
                        LastName = persistedOrder.CustomerLastName
                    }
                };

                if (persistedOrder.CustomerAddress != null)
                {
                    order.Customer.Address = JsonConvert.DeserializeObject<Address>(persistedOrder.CustomerAddress);
                }
            }

            return order;
        }
    }
}