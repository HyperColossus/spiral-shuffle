using Imlight.CoreLib.Game;

internal static class SpellRewardTests {
    internal static int Run() {
        SpellRewardQueue queue = new();
        Require(queue.Current is null, "A reward popup appeared without a drop.");
        RewardSpell first = new(101, "|SpellNames|Firecat", "Fire", "1", 75, 3);
        RewardSpell second = new(102, "Pigsie", "Life", "4", 90, 3);
        queue.Enqueue([first, first, second]);
        SpellRewardNotice notice = queue.Current!;
        Require(notice.Spell == first && notice.Number == 1 && notice.Total == 2, "Multiple drops were not queued correctly.");
        Require(queue.Current == notice, "Polling replayed the reveal with a new token.");
        string[] fields = notice.Payload.Split('|');
        Require(fields.Length == 10 && Uri.UnescapeDataString(fields[3]) == first.DisplayName,
            "A localization key broke the reward message.");
        Require(!queue.Acknowledge(0) && !queue.Acknowledge(-1) && queue.Current == notice,
            "An unrelated acknowledgement discarded a reward.");
        Require(queue.Acknowledge(notice.Token) && queue.Current!.Spell == second && queue.Current.Number == 2,
            "Dismissing the first drop skipped the second one.");
        Require(!queue.Acknowledge(notice.Token), "A duplicate acknowledgement discarded the next card.");
        queue.Enqueue([new(103, "Another spell", "Ice", "2", 80, 1)]);
        Require(queue.Acknowledge(queue.Current!.Token) && queue.Current!.Spell.TemplateId == 103,
            "A new battle replaced an undisplayed drop.");
        Require(queue.Acknowledge(queue.Current!.Token) && queue.Current is null, "The final card could not be dismissed.");
        Require(SpellRewardQueue.PipCost(2, false, 1, new Dictionary<string, int> { ["death"] = 2, ["fire"] = 0 }) == "2 + 1 shadow + 2 death",
            "The popup omitted a special pip requirement.");
        Require(SpellRewardQueue.PipCost(0, true, 0, new Dictionary<string, int>()) == "X", "An X-pip spell displayed a fixed cost.");
        Console.WriteLine("Passed 11 reward queue checks: confirmed rewards, batches, safe metadata, acknowledgements and pip costs.");
        return 11;
    }

    private static void Require(bool condition, string message) {
        if (!condition) { throw new InvalidDataException(message); }
    }
}
