using Imcodec.ObjectProperty.TypeCache;

// Edit the original BIND field so unrelated and unknown item data survives.
internal static class CrownPricing {
    internal static bool Apply(BindDocument document) {
        BindProperty? price = document.Value.Find<WizItemTemplate>("m_creditsCost");
        if (price is null || price.Data.All(value => value == 0)) return false;
        if (price.Data.Length != 4 || price.DataBits != 32)
            throw new InvalidDataException("Unexpected Crown price field layout.");
        document.Value.Set<WizItemTemplate>("m_creditsCost", price with { Data = new byte[4] });
        return true;
    }
}
