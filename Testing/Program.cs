using MoDuel.Flow;
using MoDuel.State;
using Testing;


DuelState state = CreateDuel.CreateState();

using (DuelFlow flow = new(state)) {

    flow.StartLoop();

    while (state.Ongoing) {

        Display.Field(state.Field);
        Display.PlayerStats(state.Player1);
        Display.PlayerStats(state.Player2);

        var entry = Console.ReadLine();
        if (!CommandReader.ReadCommand(flow, state.CurrentTurn.Owner, entry ?? ""))
            break;
    }

    flow.StopLoop();

}

Console.WriteLine("Done");
