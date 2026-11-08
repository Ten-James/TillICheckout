using TillICheckout.Nodes;

namespace TillICheckout;

class Program
{
    static void Main(string[] args)
    {
        var context = new TILContext();

        context.Declare("= / (oii)");
        context.Declare("kaja / i");
        context.Declare("muj_bratr / ((it)w)");

        context.Proof("'kaja");

        context.Proof("'muj_bratr@w,t");

        context.Proof("['= 'muj_bratr@w,t 'kaja]");

    }
}