using ClassLibrary.Classes.GQLObjects;
using ShopifySharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Services.Interfaces
{
    public interface ICustomCustomerService
    {
        /// <summary>
        /// Updates the tags of a given customer by appending a new tag.
        /// </summary>
        /// <param name="cust">The customer to update.</param>
        /// <param name="tag">The tag to add to the customer's existing tags.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateCustomerTagsAsync(ShopifySharp.Customer cust, string tag);

        /// <summary>
        /// Fetches all customers from the Shopify store.
        /// </summary>
        /// <returns>A list of all customers.</returns>
        Task<List<CustomerFetch>> FetchAllCustomersAsync();
    }
}
