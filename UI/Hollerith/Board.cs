using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Esoteric.UI;
using Esoteric.Hollerith;
using Esoteric.Hollerith.Defaults;
using Esoteric.Hollerith.Presentation;
using Lynx.Models;
using Lynx.UI.Dialogs;

namespace Lynx.UI.Hollerith
{
    public class Board : DefaultBoard<Card>
    {
        public DataColumn Column { get; set; }

        public override string Title
        {
            get { return Column.ColumnName; }
        }

        public override IBin<Card> Deck
        {
            get { return deck ?? (deck = GenerateBin()); }
        }
        IBin<Card> deck;

        public override IBin<Card> Create()
        {
            var candidate = GenerateBin();
            if (candidate.Configure())
                return candidate;

            return null;
        }

        public void Initialize(HollerithUsage usage, IList<Card> cards)
        {
            // Special handling for booleans
            if (Column.DataType == typeof(bool) || Column.DataType == typeof(bool?))
            {
                // Create the standard bins
                Bins.Add(new Bin<bool?> { Column = Column, ValueToSet = true });
                Bins.Add(new Bin<bool?> { Column = Column, ValueToSet = false });
                Bins.Add(new Bin<bool?> { Column = Column, ValueToSet = null });
            }

            // Generate bins based on distinct values
            else if (usage.SelectedFillOption != BoardFillOptions.None)
            {
                var query = from c in cards
                            group c by c[Column.ColumnName] into g
                            select g.Key;

                foreach (var k in query)
                {
                    if ( !string.IsNullOrEmpty(k) )
                    {
                        Bins.Add(GenerateBin(k));
                    }
                }
            }

            // Fill the deck if not populating
            if (usage.SelectedFillOption != BoardFillOptions.PopulateBins)
                Deck.Load(cards);

            // Parcel out the cards based on bin value
            else
            {
                var query = from c in cards
                            group c by c[Column.ColumnName] into g
                            select g;

                foreach (var g in query)
                {
                    IBin<Card> bin;
                    if (string.IsNullOrEmpty(g.Key))
                        bin = Deck;
                    else
                        bin = Bins.Cast<IDataColumnBin>().FirstOrDefault(b => b.ValueAsString == g.Key) as IBin<Card>;

                    if (bin != null)
                        bin.Load(g);
                }
            }

        }

        private IBin<Card> GenerateBin(string value = null)
        {
            Type genericListType = typeof(Bin<>).MakeGenericType(Column.DataType);
            IDataColumnBin instance = Activator.CreateInstance(genericListType) as IDataColumnBin;

            instance.Column = Column;
            if ( !string.IsNullOrEmpty(value))
                instance.ValueAsString = value;

            return instance as IBin<Card>;
        }
    }
}
