namespace Area51Elevator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Elevator elevator = new Elevator();
            Thread elevatorThread = new Thread(elevator.RunElevator);
            elevatorThread.Start();

            Agent agent1 = new Agent("Agent 1", SecurityLevel.Confidential, elevator);
            Agent agent2 = new Agent("Agent 2", SecurityLevel.Secret, elevator);

            Thread agentThread1 = new Thread(agent1.UseElevator);
            Thread agentThread2 = new Thread(agent2.UseElevator);

            agentThread1.Start();
            agentThread2.Start();

            agentThread1.Join();
            agentThread2.Join();
            elevatorThread.Abort(); 
        }
    }

    public enum Floor
    {
        G,
        S,
        T1,
        T2
    }

    public enum SecurityLevel
    {
        Confidential,
        Secret,
        TopSecret
    }

    public class Agent
    {
        public string Name { get; }
        public SecurityLevel Clearance { get; }
        private readonly Elevator elevator;

        public Agent(string name, SecurityLevel clearance, Elevator elevator)
        {
            Name = name;
            Clearance = clearance;
            this.elevator = elevator;
        }

        public void UseElevator()
        {
            Random random = new Random();
            while (true)
            {
                Floor currentFloor = GetRandomFloor(random);
                elevator.CallElevator(currentFloor, this);
                Thread.Sleep(random.Next(1000, 5000)); 
                elevator.RequestFloor(this, GetRandomFloor(random));
            }
        }

        private Floor GetRandomFloor(Random random)
        {
            Array values = Enum.GetValues(typeof(Floor));
            return (Floor)values.GetValue(random.Next(values.Length));
        }
    }

    public class Elevator
    {
        private Floor currentFloor = Floor.G;
        private readonly object lockObject = new object();
        private bool[] floorButtonsEnabled = { true, false, false, false };
        private SecurityLevel[] floorSecurityLevels = {
        SecurityLevel.Confidential,
        SecurityLevel.Secret,
        SecurityLevel.TopSecret,
        SecurityLevel.TopSecret
    };

        public void RunElevator()
        {
            while (true)
            {
                Thread.Sleep(1000); 
                MoveToNextFloor();
            }
        }

        private void MoveToNextFloor()
        {
            lock (lockObject)
            {
                Floor nextFloor = GetNextFloor(currentFloor);
                Console.WriteLine($"Elevator moving from floor {currentFloor} to floor {nextFloor}");
                currentFloor = nextFloor;
                Console.WriteLine($"Elevator arrived at floor {currentFloor}");
                OpenDoor();
            }
        }

        private Floor GetNextFloor(Floor current)
        {
            return current switch
            {
                Floor.G => Floor.S,
                Floor.S => Floor.T1,
                Floor.T1 => Floor.T2,
                Floor.T2 => Floor.G,
                _ => Floor.G
            };
        }

        public void CallElevator(Floor floor, Agent agent)
        {
            lock (lockObject)
            {
                if (floorSecurityLevels[(int)floor] <= agent.Clearance)
                {
                    Console.WriteLine($"{agent.Name} called the elevator from floor {floor}");
                    DisableOtherButtons(floor);
                }
                else
                {
                    Console.WriteLine($"{agent.Name} doesn't have clearance to call the elevator from floor {floor}");
                }
            }
        }

        private void DisableOtherButtons(Floor floor)
        {
            for (int i = 0; i < floorButtonsEnabled.Length; i++)
            {
                floorButtonsEnabled[i] = i == (int)floor;
            }
        }

        public void RequestFloor(Agent agent, Floor floor)
        {
            lock (lockObject)
            {
                if (floorButtonsEnabled[(int)floor])
                {
                    if (floorSecurityLevels[(int)floor] <= agent.Clearance)
                    {
                        Console.WriteLine($"{agent.Name} requested floor {floor}");
                        OpenDoor();
                    }
                    else
                    {
                        Console.WriteLine($"{agent.Name} doesn't have clearance to access floor {floor}");
                    }
                }
                else
                {
                    Console.WriteLine($"No elevator call from floor {floor}");
                }
            }
        }

        private void OpenDoor()
        {
            Console.WriteLine($"Elevator door opened at floor {currentFloor}");
            Thread.Sleep(2000);
            Console.WriteLine($"Elevator door closed at floor {currentFloor}");
        }
    }

}