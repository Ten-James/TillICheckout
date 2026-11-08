namespace TillICheckout;

public class StringUtils
{
    public class StringBox : List<string>
    {
        public int Height => Count;
        public int Width => this[0].Length;

        public StringBox(string raw): base()
        {
            var rows = raw.Split("\n");
            var max_width = rows.Aggregate(0, (max, curr) => Math.Max(max, curr.Length));
            AddRange(rows.Select(x => x.PadRight(max_width)));
        }

        public void ResizeWidth(int to)
        {
            for (var i = 0; i < this.Count; i++)
            {
                this[i] = this[i].PadRight(to);
            }
        }

        public void AddColumn(string col)
        {
            for (var i = 0; i < this.Count; i++)
            {
                this[i] += col;
            }
        }
        public void AddColumn(string first,string col)
        {
            this[0] += first;
            for (var i = 1; i < this.Count; i++)
            {
                this[i] += col;
            }
        }

        public void AddRow(string row)
        {
            Add(row.PadRight(Width));
        }

        public void ResizeHeight(int to)
        {
            var idx = Height - 1;
            while (idx >= 0)
            {
                if (this[idx].Replace("|"," ").Trim().Length == 0)
                    break;
                idx--;
            }

            while (Height < to)
            {
                Insert(idx, this[idx]);
            }
        }

        public void Join(StringBox other)
        {
            if (other.Height < Height)
                other.ResizeHeight(Height);
            if (Height < other.Height)
                ResizeHeight(other.Height);
            for (var i = 0; i < this.Count; i++)
            {
                this[i] = this[i] + other[i];
            }
        }

        public string String()
        {
            return string.Join("\n", this);
        }
    }
}