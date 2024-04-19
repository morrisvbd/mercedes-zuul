using System;
using System.Reflection.Metadata;

class Game
{
	// Private fields
	private Parser parser;
	private Player player;
	private bool keyUsed = false;
	Item key = new Item(15, "autosleutel");

	Item medkit = new Item(20, "medicijnen");
	// Constructor
	public Game()
	{
		parser = new Parser();
		player = new Player();
		CreateRooms();
	}

	// Initialise the Rooms (and the Items)
	private void CreateRooms()
	{
		// Create the rooms
		Room buiten = new Room("buiten voor de ingang van de mercedes museum");
		Room receptie = new Room("bij de receptie van de mercedes museam");
		Room beginzaal = new Room("in de zaal waar het begon bij mercedes");
		Room laatstezaal = new Room("in de zaal waar de populairste mercedessen staan van alle tijden");
		Room extrazaal = new Room("in de zaal wat mercedes nog meer deed");
		Room hoofdzaal = new Room("in de hoofdhal van de mercedes museum");
		Room omhoog = new Room("op de 1ste verdieping van de mercedes museum");
		Room opslagkamer = new Room("in de opslag kamer");
		// Initialise room exits
		buiten.AddExit("oost", receptie);
		buiten.AddExit("zuid", laatstezaal);
		buiten.AddExit("west", beginzaal);

		receptie.AddExit("west", buiten);
		receptie.AddExit("door", hoofdzaal);

		hoofdzaal.AddExit("receptie", receptie);
		hoofdzaal.AddExit("omhoog", omhoog);

		omhoog.AddExit("omlaag", hoofdzaal);
		omhoog.AddExit("opslag", opslagkamer);

		opslagkamer.AddExit("hoofdzaal", hoofdzaal);

		beginzaal.AddExit("oost", buiten);

		laatstezaal.AddExit("noord", buiten);
		laatstezaal.AddExit("oost", extrazaal);

		extrazaal.AddExit("west", laatstezaal);

        opslagkamer.Chest.Put("medicijnen", medkit);
		laatstezaal.Chest.Put("autosleutel", key);
		// Start game outside
		player.CurrentRoom = buiten;
	}

	//  Main play routine. Loops until end of play.
	public void Play()
	{
		PrintWelcome();

		// Enter the main command loop. Here we repeatedly read commands and
		// execute them until the player wants to quit.
		bool finished = false;
		while (!finished)
		{
			Command command = parser.GetCommand();
			finished = ProcessCommand(command);
		}
		Console.WriteLine("Bedankt voor het spelen.");
		Console.WriteLine("druk [Enter] om verder te gaan.");
		Console.ReadLine();
	}

	// Print out the opening message for the player.
	private void PrintWelcome()
	{
		Console.WriteLine();
		Console.WriteLine("Welkom bij dit spel!");
		Console.WriteLine("Mercedes-zuul is een nieuwe text avonturen en het doel van de spel is de autosleutel te vinden u hp is 100 en per kamer gaat er 20hp af zoek goed rond en de autosleutel is overal te gebruiken!");
		Console.WriteLine("Typ 'hulp' voor als u wilt weten welke commandos mag gebruiken.");
		Console.WriteLine();
		Console.WriteLine(player.CurrentRoom.GetLongDescription(player));
	}

	// Given a command, process (that is: execute) the command.
	// If this command ends the game, it returns true.
	// Otherwise false is returned.
private bool ProcessCommand(Command command)
{
	bool wantToQuit = false;

	if (!player.IsAlive() && command.CommandWord != "Stoppen")
	{
		Console.WriteLine("U bent dood gebloed helaas u heeft veloren...");
		Console.WriteLine("U kunt alleen de commando:");
		Console.WriteLine("Stoppen");
		return wantToQuit;
	}

	if (keyUsed && command.CommandWord != "stoppen") 
	{
		Console.WriteLine("u heeft de spel gewonnen de enigste toegestaande commando toegestaan is 'stoppen'.");
		return wantToQuit;
	}

	if (command.IsUnknown())
	{
		Console.WriteLine("Ik weet niet wat u bedoelt!...");
		return wantToQuit;
	}

    switch (command.CommandWord)
    {
        case "hulp":
            PrintHelp();
			break;

        case "ga":
            GoRoom(command);
            break;
		
		case "stoppen":
			wantToQuit = true;
			break;

		case "gebruik":
			UseItem(command, out keyUsed); 
			break;

        case "kijkrond":
            Look();
            break;

		case "oppakken":
			Take(command);
			break;

		case "laatvallen":
			Drop(command);
			break;

        case "status":
            Health();
            break;	
	}

	return wantToQuit;
}

	// ######################################
	// implementations of user commands:
	// ######################################
	
	// Print out some help information.
	// Here we print the mission and a list of the command words.
	private void PrintHelp()
	{
		Console.WriteLine("U bent verdwaald en alleen.");
		Console.WriteLine("U loopt rond bij de mercedes dealer.");
		Console.WriteLine();
		// let the parser print the commands
		parser.PrintValidCommands();
	}

	private void Look()
	{
		Console.WriteLine(player.CurrentRoom.GetLongDescription(player));

		Dictionary<string, Item> roomItems = player.CurrentRoom.Chest.GetItems();
		if (roomItems.Count > 0)
		{
			Console.WriteLine("Items in deze kamer:");
			foreach (var itemEntry in roomItems)
			{
				Console.WriteLine($"{itemEntry.Value.Description} - ({itemEntry.Value.Weight} kg)");
			}
		}
	}


	private void Take(Command command)
	{
		if (!command.HasSecondWord())
		{
			Console.WriteLine("Pak wat op?");
			return;
		}

		string itemName = command.SecondWord.ToLower();

		bool success = player.TakeFromChest(itemName);

	}

	private void Drop(Command command)
	{
		if (!command.HasSecondWord())
		{
			Console.WriteLine("Wat laten vallen?");
			return;
		}

		string itemName = command.SecondWord.ToLower();

		bool success = player.DropToChest(itemName);


	}

	private void Health()
	{
		Console.WriteLine($"U hp is: {player.GetHealth()}");

		Dictionary<string, Item> items = player.GetItems();

		if (items.Count > 0)
		{
			Console.WriteLine("U huidige items:");

			// Iterate over elk item in player zijn inv
			foreach (var itemEntry in items)
			{
				Console.WriteLine($"- {itemEntry.Key}: Weight {itemEntry.Value.Weight}");
			}
		}
		else
		{
			Console.WriteLine("U hebt geen spullen opzak .");
		}
	}

	
	// Try to go to one direction. If there is an exit, enter the new
	// room, otherwise print an error message.
	private void GoRoom(Command command)
	{
		if(!command.HasSecondWord())
		{
			// if there is no second word, we don't know where to go...
			Console.WriteLine("Waarheen?");
			return;
		}

		string direction = command.SecondWord;

		// Try to go to the next room.
		Room nextRoom = player.CurrentRoom.GetExit(direction);
		if (nextRoom == null)
		{
			Console.WriteLine("Er is geen deur naar "+direction+"!");
			return;
		}

		player.Damage(20);
		player.CurrentRoom = nextRoom;
		Console.WriteLine(player.CurrentRoom.GetLongDescription(player));
		if (player.CurrentRoom.GetExit("door") != null)
		{
			Console.WriteLine("U heeft een deur gevonden bij de receptie van de mercedes museum het lijkt richting de hoofdzaal te gaan van de mercedes museum.");
		}
		
		if (!player.IsAlive()) 
		{
			Console.WriteLine("U visie wordt wazig en u wonden bloeden te erg u bent dood! ..");
		}
	}

    private void UseItem(Command command, out bool keyUsed)
    {
        if (!command.HasSecondWord())
        {
            Console.WriteLine("Gebruik wat?");
            keyUsed = false;
            return;
        }

        string itemName = command.SecondWord.ToLower();

        bool itemUsed = player.Use(itemName, out keyUsed);

        if (itemUsed)
        {
            if (keyUsed)
            {
                this.keyUsed = true; 
                Console.WriteLine("U heeft een auto gevonden!");
				Console.WriteLine("kijk de eigenaar van mercedes komt eraan gelopen!");
				Console.WriteLine("hij feleciteer u en de auto mag u houden");
				Console.WriteLine("het is een mercedes gle 63 s amg u stapt in en rijd weg");
				Console.WriteLine("Gefeliciteerd u heeft het spel gewonnen.");
            }
        }
    }
}




