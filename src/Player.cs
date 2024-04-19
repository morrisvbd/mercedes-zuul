class Player
{
    public Room CurrentRoom { get; set; }
    private int health;
    private Inventory backpack;

    // Constructor
    public Player()
    {
        CurrentRoom = null; 
        health = 100;
        backpack = new Inventory(25);
    }
    
    public Dictionary<string, Item> GetItems()
    {
        return backpack.GetItems();
    }

    public int GetHealth()
    {
        return health;
    }


    public void Damage(int amount)
    {
        health -= amount;
        if (health < 0)
            health = 0;
    }

    public void Heal(int amount)
    {
        health += amount;
        if (health > 100)
            health = 100;
    }

    public bool IsAlive()
    {
        return health > 0;
    }

    public bool TakeFromChest(string itemName)
    {
        Item item = CurrentRoom.Chest.Get(itemName);

        if (item != null) // item gevonden in kamer
        {
            bool success = backpack.Put(itemName, item);
            
            if (success)
            {
                // Remove the item from the chest
                CurrentRoom.Chest.Get(itemName);
                Console.WriteLine($"u pakte de {itemName} op.");
                return true;
            }
            else
            {
                CurrentRoom.Chest.Put(itemName, item);
                Console.WriteLine("U kunt het niet meenemen het is te zwaar.");
                return false;
            }
        }
        else
        {
            Console.WriteLine($"er is hier geen {itemName} hier.");
            return false;
        }
    }

    public bool DropToChest(string itemName)
    {
        Item item = backpack.Get(itemName);

        if (item != null) // Item found in inventory
        {
            bool success = CurrentRoom.Chest.Put(itemName, item);

            if (success)
            {
                backpack.RemoveWeight(item.Weight);

                Console.WriteLine($"u heeft {itemName} laten vallen.");
                return true;
            }
            else
            {
                backpack.Put(itemName, item);
                return false;
            }
        }
        else
        {
            Console.WriteLine($"U heeft  {itemName} niet opzak.");
            return false;
        }
    }


    public bool Use(string itemName, out bool keyUsed)
    {
        keyUsed = false; 
        if (backpack.GetItems().ContainsKey(itemName))
        {
            if (itemName.ToLower() == "medicijen")
            {
                health += 25;
                if (health > 100)
                {
                    health = 100;
                }

                backpack.GetItems().Remove(itemName);
                backpack.RemoveWeight(20);
                Console.WriteLine($"U heeft u medicijnen gebruikt u hp is nu {health}.");
                return true; 
            }
            else if (itemName.ToLower() == "sleutel")
            {
                keyUsed = true; // Set keyUsed to true
                backpack.GetItems().Remove(itemName);
                Console.WriteLine("U heeft de autosleutel gebruikt.");
                backpack.RemoveWeight(15);
                return true; 
            }
            else
            {
                Console.WriteLine("U kunt de item niet gebruiken.");
                return false; 
            }
        }
        else
        {
            Console.WriteLine("u heeft niet die item opzak.");
            return false; 
        }
    }

}

