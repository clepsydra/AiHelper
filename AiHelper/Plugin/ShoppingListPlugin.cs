using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.CvEnum;
using Microsoft.SemanticKernel;

namespace AiHelper.Plugin
{
    internal class ShoppingListPlugin
    {
        private List<string> shoppingList = [];

        private string ShoppingListFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AiHelper", "ShoppingList.txt");

        public ShoppingListPlugin()
        {
            if (File.Exists(ShoppingListFileName))
            {
                string[] lines = File.ReadAllLines(ShoppingListFileName);
                foreach (string line in lines)
                {
                    shoppingList.Add(line);
                }
            }
        }

        [KernelFunction]
        [Description(@"Add an item to the shopping list.
Paramters:
- The item to add.
Return value: true if the item was added, false if the item is already part of the list.")]
        public async Task<bool> AddItemToShoppingList(string text)
        {
            if (shoppingList.Any(item => item.Equals(text, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            shoppingList.Add(text);
            SaveList();
            return true;
        }

        [KernelFunction]
        [Description(@"Get the contents of the shopping list.")]
        public async Task<string> GetShoppingList()
        {
            return string.Join("\r\n", shoppingList);
        }

        [KernelFunction]
        [Description(@"Delete an item to the shopping list.
Paramters:
- The item to delete.
Return value: true if the item was deleted, false if the item could not be found in the list.")]
        public async Task<bool> DeleteItemFromShoppingListt(string text)
        {
            var matchingItem = shoppingList.FirstOrDefault(item => item.Equals(text, StringComparison.OrdinalIgnoreCase));
            if (matchingItem == null)
            {
                return false;
            }

            shoppingList.Remove(matchingItem);
            SaveList();
            return true;
        }

        private void SaveList()
        {
            if (File.Exists(ShoppingListFileName))
            {
                File.Delete(ShoppingListFileName);
            }

            if (!Directory.Exists(Path.GetDirectoryName(ShoppingListFileName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ShoppingListFileName));
            }

            File.WriteAllLines(ShoppingListFileName, shoppingList);
        }
    }
}
