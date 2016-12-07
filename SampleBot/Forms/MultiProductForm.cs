using OAChatBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OAChatBot.Extensions;
using OAChatBot.Services;

namespace OAChatBot.Forms
{
    [Serializable]
    public class MultiProductForm
    {
        private const string ProductSelectionCancelled = "You have cancelled the selection. Please add new item or type **done** to create an order with existing items in cart.";

        public BbProduct SelectedBbProduct;

        private List<string> _products;

        public List<string> Products
        {
            get
            {
                return _products ?? new List<string>();
            }
            set { _products = value; }
        }


        [Optional]
        [Template(TemplateUsage.NotUnderstood, "I do not understand \"{0}\".", "Try again, I don't get \"{0}\".")]
        [Template(TemplateUsage.NoPreference, "None")]
        [Template(TemplateUsage.EnumSelectOne, "Please select a product {||}", ChoiceStyle = ChoiceStyleOptions.PerLine)]
        public string Product { get; set; }

        public IForm<MultiProductForm> BuildForm()
        {
            return new FormBuilder<MultiProductForm>()
                    .Field(new FieldReflector<MultiProductForm>(nameof(Product))
                            .SetType(null)
                           .SetAllowsMultiple(false)
                           .SetOptional(true)
                           .SetDefine((state, field) =>
                            {
                                foreach (var prod in Products)
                                    field
                                        .AddDescription(prod, prod)
                                        .AddTerms(prod, prod);

                                return Task.FromResult(true);
                            }))
                    .Confirm(async (state) =>
                    {
                        if (state.Product == null)
                            return new PromptAttribute("Are you sure you do not want to select any product?");

                        return new PromptAttribute($"Are you sure you want to buy {state.Product}?");
                    })
                    .OnCompletionAsync(async (context, productForm) =>
                    {
                        await SelectionComplete(context, productForm);
                    })
                    .Build();
        }

        private async Task SelectionComplete(IDialogContext context, MultiProductForm result)
        {
            var selectedProd = result;

            if (selectedProd?.Product == null)//Cancelled
            {
                await context.PostAsyncCustom(ProductSelectionCancelled);
                return;
            }

            Dictionary<string, BbProduct> userProds;
            context.UserData.TryGetValue("promptData", out userProds);

            if (userProds == null || !(userProds.Any((userProd) => string.Compare(selectedProd.Product, userProd.Key, StringComparison.OrdinalIgnoreCase) == 0)))
            {
                await context.PostAsyncCustom("Sorry, some error in the product selection.");
                return;
            }

            SelectedBbProduct = userProds.First(userProd => string.Compare(selectedProd.Product, userProd.Key, StringComparison.OrdinalIgnoreCase) == 0).Value;// await productSel;

            if (SelectedBbProduct == null)
            {
                await context.PostAsyncCustom("Sorry, something went wrong. Please select the product again.");
                return;
            }

            var prodQty = SelectedBbProduct.QuantityPerUnit ?? "1";
            var prodSelected = $"{SelectedBbProduct.ProductName}-{prodQty}- Rs.{SelectedBbProduct.UnitPrice}";

            UserContext userContext = null;
            context.UserData.TryGetValue("userContext", out userContext);

            var message = await ServiceHandler.PersisitToCart(SelectedBbProduct, userContext);
            await context.PostAsyncCustom(message);


        }
    }
}