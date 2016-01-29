using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refactoring
{
    public class Tusc
    {
        private static List<User> users;
        private static List<Product> products;
        private const int EXIT_CODE = 7;

        public static void Start(List<User> usrs, List<Product> prods)
        {
            users = usrs;
            products = prods;

            displaySplash();

            User user = loginProcess();

            if (user != null)
            {
                // Show welcome message
                displayLoginSuccess(user);
                displayBalance(user);

                // Show product list
                int selection;
                bool changesMade = false;
                do
                {
                    displayProducts();
                    selection = readSelection();
                    changesMade = processSelection(selection, user);
                } while (selection != EXIT_CODE);

                if (changesMade)
                {
                    updateBalance(user);
                    saveChanges();
                }
            }
            // Prevent console from closing
            Console.WriteLine();
            Console.WriteLine("Press Enter key to exit");
            Console.ReadLine();
        }

        private static bool processSelection(int selection, User user)
        {
            bool changesMade = false;
            if (selection != EXIT_CODE)
            {
                displayOrderConfirmation(selection, user);
                int quantity = readQty();

                if (validateQuantity(quantity) && checkBalance(quantity, user, selection) && checkQuantity(quantity, selection))
                {
                    changesMade = true;
                    user.Bal -= products[selection].Price * quantity;
                    products[selection].Qty = products[selection].Qty - quantity;

                    displayPurchaseConfirmation(quantity, user, selection);
                }
            }
            return changesMade;
        }

        private static User loginProcess()
        {
            User user = null;

            bool sentinel = false;
            do
            {
                user = attemptLogin();
                if (user == null || user.isValid)
                {
                    sentinel = true;
                }
            }
            while (!sentinel);

            return user;
        }

        private static void saveChanges()
        {
            string json = JsonConvert.SerializeObject(users, Formatting.Indented);
            File.WriteAllText(@"Data/Users.json", json);

            // Write out new quantities
            string json2 = JsonConvert.SerializeObject(products, Formatting.Indented);
            File.WriteAllText(@"Data/Products.json", json2);
        }

        private static void updateBalance(User user)
        {
            foreach (var usr in users)
            {
                // Check that name and password match
                if (usr.Name == user.Name && usr.Pwd == user.Pwd)
                {
                    usr.Bal = user.Bal;
                }
            }
        }

        private static void displayPurchaseConfirmation(int quantity, User user, int selection)
        {
            prepConsole(ConsoleColor.Green);
            Console.WriteLine("You bought " + quantity + " " + products[selection].Name);
            Console.WriteLine("Your new balance is " + user.Bal.ToString("C"));
            Console.ResetColor();
        }

        private static bool validateQuantity(int quantity)
        {
            bool validQuantity = true;
            if (quantity <= 0)
            {
                prepConsole(ConsoleColor.Yellow);
                Console.WriteLine("Purchase cancelled");
                Console.ResetColor();
                validQuantity = false;
            }
            return validQuantity;
        }

        private static bool checkQuantity(int quantity, int selection)
        {
            bool validQuantity = true;
            if (products[selection].Qty <= quantity)
            {
                prepConsole(ConsoleColor.Red);
                Console.WriteLine("Sorry, " + products[selection].Name + " is out of stock");
                Console.ResetColor();
                validQuantity = false;
            }
            return validQuantity;
        }

        private static bool checkBalance(int quantity, User user, int selection)
        {
            bool balanceAvailable = true;
            if (user.Bal - products[selection].Price * quantity < 0)
            {
                prepConsole(ConsoleColor.Red);
                Console.WriteLine("You do not have enough money to buy that.");
                Console.ResetColor();
                balanceAvailable = false;
            }
            return balanceAvailable;
        }

        private static int readQty()
        {
            Console.WriteLine("Enter amount to purchase:");
            int qty = readInt();
            return qty;
        }

        private static void displayOrderConfirmation(int selection, User user)
        {
            Console.WriteLine();
            Console.WriteLine("You want to buy: " + products[selection].Name);
            Console.WriteLine("Your balance is " + user.Bal.ToString("C"));
        }

        private static int readSelection()
        {
            Console.WriteLine("Enter a number:");
            int num = readInt();
            return num - 1;
      
        }

        private static int readInt()
        {
            string answer = Console.ReadLine();
            int num;
            bool isNum = int.TryParse(answer, out num);
            if (!isNum)
            {
                throw new FormatException("Could not parse input");
            }
            return num;
        }

        private static void displayProducts()
        {
            // Prompt for user input
            Console.WriteLine();
            Console.WriteLine("What would you like to buy?");
            for (int i = 0; i < 7; i++)
            {
                Product prod = products[i];
                Console.WriteLine(i + 1 + ": " + prod.Name + " (" + prod.Price.ToString("C") + ")");
            }
            Console.WriteLine(products.Count + 1 + ": Exit");

        }

        private static void displayBalance(User user)
        {
            Console.WriteLine();
            Console.WriteLine("Your balance is " + user.Bal.ToString("C"));
        }

        private static void displayLoginSuccess(User user)
        {
            prepConsole(ConsoleColor.Green);
            Console.WriteLine("Login successful! Welcome " + user.Name + "!");
            Console.ResetColor();
        }

        private static User attemptLogin()
        {
            Console.WriteLine();
            Console.WriteLine("Enter Username:");
            String name = Console.ReadLine();
            Console.WriteLine("Enter Password:");
            String pwd = Console.ReadLine();

            User attemptedUser = null;
            if (!string.IsNullOrEmpty(name))
            {
                attemptedUser = getUser(name, pwd);
            }

            if (attemptedUser != null && !attemptedUser.isValid)
            {
                displayAuthenticationError();
            }

            return attemptedUser;
        }


        private static void displayAuthenticationError()
        {

            Console.WriteLine("You entered an invalid username/password.");
            Console.ResetColor();
        }

        private static void displaySplash()
        {
            // Write welcome message
            Console.WriteLine("Welcome to TUSC");
            Console.WriteLine("---------------");
        }

        private static User getUser(String name, String pwd)
        {
            bool validUser = false;

            User attemptedUser = new User();
            attemptedUser.isValid = false;

            int userCount = 0;
            while (userCount < users.Count && !validUser)
            {
                User user = users[userCount++];
                if (user.Name == name && user.Pwd == pwd)
                {
                    attemptedUser = new User();
                    attemptedUser.Name = name;
                    attemptedUser.Pwd = pwd;
                    attemptedUser.Bal = user.Bal;
                    attemptedUser.isValid = true;
                    validUser = true;
                }
            }

            return attemptedUser;
        }

        private static void prepConsole(ConsoleColor color)
        {
            Console.Clear();
            Console.ForegroundColor = color;
            Console.WriteLine();
        }
    }
}
