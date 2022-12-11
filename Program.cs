using static Toolbox;

namespace lekcja_gra
{
    public class Engine
    {
        private static readonly bool DEBUG_ON = false;

        public struct Gracz
        {
            public enum Mode
            {
                Manual,
                Auto
            }
            public Mode mode;
            public Symbol symbol;
        }
        public enum Symbol
        {
            Kolko,
            Pusty,
            Krzyzyk,

            PustySelected,
            KolkoSelected,
            KrzyzykSelected,

            KolkoSolid,
            KrzyzykSolid,

            KolkoSolidSelected,
            KrzyzykSolidSelected
        }

#pragma warning disable IDE0044
        private Symbol[,] mapa;
        private AI bot;
#pragma warning restore IDE0044
        private (int x, int y) RozmiarMapy;
        private (Gracz pierwszy, Gracz drugi) gracze;
        private Gracz aktualny_gracz, winner;
        private (int kolko, int krzyzyk) score;
        public Engine(int x_map, int y_map, bool czy_pierwszy_gracz_to_komputer, bool czy_drugi_gracz_to_komputer, int opoznienie_ai = 500)
        {
            if(x_map < 3 || y_map < 3)
            {
                throw new ArgumentException("Mapa nie może być mniejsza od 3x3");
            }
            if(x_map > Math.Round((double)Console.BufferHeight/2) - 2 || y_map > Math.Round((double)Console.BufferWidth/2) - 1) // wysokosc -2 z racji górnej linii z wynikami i dolnej ramki mapy
            {
                throw new ArgumentException($"Mapa nie może być większa od {Math.Round((double)Console.BufferHeight/2) - 2}x{Math.Round((double)Console.BufferWidth/2) - 1}");
            }

            if (DEBUG_ON)
            {
                Debug($"{x_map} {y_map} {Math.Round((double)Console.BufferHeight / 2) - 2} {Math.Round((double)Console.BufferWidth / 2) - 1}");
                Czekaj();
            }

            RozmiarMapy.x = x_map;
            RozmiarMapy.y = y_map;
            mapa = new Symbol[RozmiarMapy.x, RozmiarMapy.y];
            gracze.pierwszy = new() { mode = czy_pierwszy_gracz_to_komputer ? Gracz.Mode.Auto : Gracz.Mode.Manual, symbol = Symbol.Kolko };
            gracze.drugi = new() { mode = czy_drugi_gracz_to_komputer ? Gracz.Mode.Auto : Gracz.Mode.Manual, symbol = Symbol.Krzyzyk };
            aktualny_gracz = gracze.pierwszy;
            for (int x = 0; x < RozmiarMapy.x; x++)
            {
                for (int y = 0; y < RozmiarMapy.y; y++)
                {
                    mapa[x, y] = Symbol.Pusty;

                }
            }
            bot = new(ref this.mapa, ref this.RozmiarMapy, opoznienie_ai);
        }

        private class AI
        {
#pragma warning disable IDE0044
            private Symbol[,] mapa;
            private int Delay;
#pragma warning restore IDE0044
            private (int x, int y) RozmiarMapy;
            
            public AI(ref Symbol[,] mapa, ref (int x, int y) rozmiarMapy, int delay)
            {
                this.mapa = mapa;
                this.RozmiarMapy = rozmiarMapy;
                this.Delay = delay;
            }

            private static Symbol OdwrotnySymbol(Symbol symbol)
            {
                if (symbol == Symbol.Kolko) return Symbol.Krzyzyk;
                else return Symbol.Kolko;
            }

            private (int x, int y, bool effective) NajblizszaKombinacja(int x, int y, bool najblizsza = false)
            {
                int wiersz = x, kolumna = y;
                Symbol pusty = Symbol.Pusty, symbol = mapa[x, y];
                (int x, int y) p1 = (-1, -1), p2 = (-1, -1);
                if (wiersz > 0 && wiersz + 1 < RozmiarMapy.x) //pion dla punktu w srodku
                {
                    if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz - 1, kolumna] == pusty &&
                        mapa[wiersz + 1, kolumna] == pusty)
                    {
                        p2 = (wiersz - 1, kolumna);
                    }
                    else if(mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz - 1, kolumna] == symbol &&
                        mapa[wiersz + 1, kolumna] == pusty)
                    {
                        p1 = (wiersz + 1, kolumna);
                    }
                    else if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz - 1, kolumna] == pusty &&
                        mapa[wiersz + 1, kolumna] == symbol)
                    {
                        p1 = (wiersz - 1, kolumna);
                    }
                }
                if (p1 != (-1, -1)) return (p1.x, p1.y, true);

                if (wiersz + 2 < RozmiarMapy.x)               //pion dla punktu u gory
                {
                    if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz + 1, kolumna] == pusty &&
                        mapa[wiersz + 2, kolumna] == pusty)
                    {
                        p2 = (wiersz + 1, kolumna);
                    }
                    else if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz + 1, kolumna] == symbol &&
                        mapa[wiersz + 2, kolumna] == pusty)
                    {
                        p1 = (wiersz + 2, kolumna);
                    }
                    else if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz + 1, kolumna] == pusty &&
                        mapa[wiersz + 2, kolumna] == symbol)
                    {
                        p1 = (wiersz + 1, kolumna);
                    }
                }
                if (p1 != (-1, -1)) return (p1.x, p1.y, true);

                if (wiersz > 1)                                //pion dla punktu na dole
                {
                    if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz - 1, kolumna] == pusty &&
                        mapa[wiersz - 2, kolumna] == pusty)
                    {
                        p2 = (wiersz - 1, kolumna);
                    }
                    else if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz - 1, kolumna] == symbol &&
                        mapa[wiersz - 2, kolumna] == pusty)
                    {
                        p1 = (wiersz - 2, kolumna);
                    }
                    else if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz - 1, kolumna] == pusty &&
                        mapa[wiersz - 2, kolumna] == symbol)
                    {
                        p1 = (wiersz - 1, kolumna);
                    }
                }
                if (p1 != (-1, -1)) return (p1.x, p1.y, true);

                if (kolumna > 0 && kolumna + 1 < RozmiarMapy.y)                          //poziom dla punktu w srodku
                {
                    if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz, kolumna - 1] == pusty &&
                        mapa[wiersz, kolumna + 1] == pusty)
                    {
                        p2 = (wiersz, kolumna - 1);
                    }
                    else if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz, kolumna - 1] == symbol &&
                        mapa[wiersz, kolumna + 1] == pusty)
                    {
                        p1 = (wiersz, kolumna + 1);
                    }
                    else if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz, kolumna - 1] == pusty &&
                        mapa[wiersz, kolumna + 1] == symbol)
                    {
                        p1 = (wiersz, kolumna - 1);
                    }
                }
                if (p1 != (-1, -1)) return (p1.x, p1.y, true);

                if (kolumna + 2 < RozmiarMapy.y)              //poziom dla punktu po lewej
                {
                    if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz, kolumna + 1] == pusty &&
                        mapa[wiersz, kolumna + 2] == pusty)
                    {
                        p2 = (wiersz, kolumna + 1);
                    }
                    else if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz, kolumna + 1] == symbol &&
                        mapa[wiersz, kolumna + 2] == pusty)
                    {
                        p1 = (wiersz, kolumna + 2);
                    }
                    else if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz, kolumna + 1] == pusty &&
                        mapa[wiersz, kolumna + 2] == symbol)
                    {
                        p1 = (wiersz, kolumna + 1);
                    }
                }
                if (p1 != (-1, -1)) return (p1.x, p1.y, true);

                if (kolumna > 1)                             //poziom dla punktu po prawej
                {
                    if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz, kolumna - 1] == pusty &&
                        mapa[wiersz, kolumna - 2] == pusty)
                    {
                        p2 = (wiersz, kolumna - 1);
                    }
                    else if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz, kolumna - 1] == symbol &&
                        mapa[wiersz, kolumna - 2] == pusty)
                    {
                        p1 = (wiersz, kolumna - 2);
                    }
                    else if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz, kolumna - 1] == pusty &&
                        mapa[wiersz, kolumna - 2] == symbol)
                    {
                        p1 = (wiersz, kolumna - 1);
                    }

                }
                if (p1 != (-1, -1)) return (p1.x, p1.y, true);

                if (kolumna + 2 < RozmiarMapy.y &&              //ukos \ dla punktu w lewym gornym rogu
                    wiersz + 2 < RozmiarMapy.x)
                {
                    if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz + 1, kolumna + 1] == pusty &&
                        mapa[wiersz + 2, kolumna + 2] == pusty)
                    {
                        p2 = (wiersz + 1, kolumna + 1);
                    }
                    else if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz + 1, kolumna + 1] == symbol &&
                        mapa[wiersz + 2, kolumna + 2] == pusty)
                    {
                        p1 = (wiersz + 2, kolumna + 2);
                    }
                    else if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz + 1, kolumna + 1] == pusty &&
                        mapa[wiersz + 2, kolumna + 2] == symbol)
                    {
                        p1 = (wiersz + 1, kolumna + 1);
                    }
                }
                if (p1 != (-1, -1)) return (p1.x, p1.y, true);

                if (wiersz > 0 && wiersz + 1 < RozmiarMapy.x &&
                    kolumna > 0 && kolumna + 1 < RozmiarMapy.y) //ukos \ dla punktu w srodku
                {
                    if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz - 1, kolumna - 1] == pusty &&
                        mapa[wiersz + 1, kolumna + 1] == pusty)
                    {
                        p2 = (wiersz - 1, kolumna - 1);
                    }
                    else if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz - 1, kolumna - 1] == symbol &&
                        mapa[wiersz + 1, kolumna + 1] == pusty)
                    {
                        p1 = (wiersz + 1, kolumna + 1);
                    }
                    else if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz - 1, kolumna - 1] == pusty &&
                        mapa[wiersz + 1, kolumna + 1] == symbol)
                    {
                        p1 = (wiersz - 1, kolumna - 1);
                    }
                }
                if (p1 != (-1, -1)) return (p1.x, p1.y, true);

                if (wiersz > 1 && kolumna > 1)                  //ukos \ dla punktu w prawym dolnym rogu
                {
                    if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz - 1, kolumna - 1] == pusty &&
                        mapa[wiersz - 2, kolumna - 2] == pusty)
                    {
                        p2 = (wiersz - 1, kolumna - 1);
                    }
                    else if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz - 1, kolumna - 1] == symbol &&
                        mapa[wiersz - 2, kolumna - 2] == pusty)
                    {
                        p1 = (wiersz - 2, kolumna - 2);
                    }
                    else if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz - 1, kolumna - 1] == pusty &&
                        mapa[wiersz - 2, kolumna - 2] == symbol)
                    {
                        p1 = (wiersz - 1, kolumna - 1);
                    }
                }
                if (p1 != (-1, -1)) return (p1.x, p1.y, true);

                if (wiersz + 2 < RozmiarMapy.x && kolumna > 1)  //ukos / dla punktu w prawym gornym rogu
                {
                    if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz + 1, kolumna - 1] == pusty &&
                        mapa[wiersz + 2, kolumna - 2] == pusty)
                    {
                        p2 = (wiersz + 1, kolumna - 1);
                    }
                    else if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz + 1, kolumna - 1] == symbol &&
                        mapa[wiersz + 2, kolumna - 2] == pusty)
                    {
                        p1 = (wiersz + 2, kolumna - 2);
                    }
                    else if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz + 1, kolumna - 1] == pusty &&
                        mapa[wiersz + 2, kolumna - 2] == symbol)
                    {
                        p1 = (wiersz + 1, kolumna - 1);
                    }
                }
                if (p1 != (-1, -1)) return (p1.x, p1.y, true);

                if (wiersz > 0 && wiersz + 1 < RozmiarMapy.x && //ukos / dla punktu w srodku
                    kolumna > 0 && kolumna + 1 < RozmiarMapy.y)
                {
                    if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz - 1, kolumna + 1] == pusty &&
                        mapa[wiersz + 1, kolumna - 1] == pusty)
                    {
                        p2 = (wiersz - 1, kolumna + 1);
                    }
                    else if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz - 1, kolumna + 1] == symbol &&
                        mapa[wiersz + 1, kolumna - 1] == pusty)
                    {
                        p1 = (wiersz + 1, kolumna - 1);
                    }
                    else if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz - 1, kolumna + 1] == pusty &&
                        mapa[wiersz + 1, kolumna - 1] == symbol)
                    {
                        p1 = (wiersz - 1, kolumna + 1);
                    }
                }
                if (p1 != (-1, -1)) return (p1.x, p1.y, true);

                if (wiersz > 1 && kolumna + 2 < RozmiarMapy.y)  //ukos / dla punktu w lewym dolnym rogu
                {
                    if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz - 1, kolumna + 1] == pusty &&
                        mapa[wiersz - 2, kolumna + 2] == pusty)
                    {
                        p2 = (wiersz - 1, kolumna + 1);
                    }
                    else if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz - 1, kolumna + 1] == symbol &&
                        mapa[wiersz - 2, kolumna + 2] == pusty)
                    {
                        p1 = (wiersz - 2, kolumna + 2);
                    }
                    else if (mapa[wiersz, kolumna] == symbol &&
                        mapa[wiersz - 1, kolumna + 1] == pusty &&
                        mapa[wiersz - 2, kolumna + 2] == symbol)
                    {
                        p1 = (wiersz - 1, kolumna + 1);
                    }
                }
                if (DEBUG_ON)
                {
                    Debug($"p1->{p1} p2->{p2}");
                    Czekaj();
                }
                if (p1 != (-1, -1)) return (p1.x, p1.y, true);

                else if (p2 != (-1, -1)) 
                {
                    if (najblizsza) return (p2.x, p2.y, false);
                    return NajblizszaKombinacja(x, y, true); 
                }
                else return (0, 0, false);
            }

            private int SzacujRuch(Symbol symbol)
            {
                bool puste = true;
                for(int x = 0; x < RozmiarMapy.x; x++)
                {
                    for(int y = 0; y < RozmiarMapy.y; y++)
                    {
                        if (mapa[x,y] != Symbol.Pusty) puste = false;
                    }
                }

                if (puste) return 2; //random

                for (int x = 0; x < RozmiarMapy.x; x++)
                {
                    for (int y = 0; y < RozmiarMapy.y; y++)
                    {
                        if (mapa[x, y] == OdwrotnySymbol(symbol))
                            if (NajblizszaKombinacja(x, y).effective)
                                return 1; //blokuj przeciwnika
                        if (mapa[x, y] == symbol)
                            if(NajblizszaKombinacja(x, y).effective)
                                return 0; //blokuj przeciwnika
                    }
                }
                return 0; //atak
            }
            public (int x, int y) WybierzPole(Symbol symbol)
            {
                int x = -1, y = -1;
                int f = SzacujRuch(symbol);
                if (f == 0) //atak
                {
                    if (DEBUG_ON)
                    {
                        Debug("atak");
                        Czekaj();
                    }
                    (int x, int y) ruch1 = (-1, -1), ruch2 = (-1, -1);
                    bool Break = false;
                    for (int xi = 0; xi < RozmiarMapy.x && !Break; xi++)
                    {
                        for (int yi = 0; yi < RozmiarMapy.y && !Break; yi++)
                        {
                            if (mapa[xi, yi] == symbol)
                            {
                                var atak = NajblizszaKombinacja(xi, yi);
                                if (atak.effective) 
                                    ruch1 = (atak.x, atak.y);
                                else 
                                    ruch2 = (atak.x, atak.y);
                            }
                        }
                    }
                    if (ruch1 != (-1, -1))
                    {
                        x = ruch1.x;
                        y = ruch1.y;
                    }
                    else if (ruch2 != (-1, -1))
                    {
                        x = ruch2.x;
                        y = ruch2.y;
                    }
                    
                }
                else if (f == 1) //blokuj przeciwnika
                {
                    if (DEBUG_ON)
                    {
                        Debug("blokuj przeciwnika");
                        Czekaj();
                    }
                    (int x, int y) ruch1 = (-1, -1), ruch2 = (-1, -1);
                    bool Break = false;
                    for (int xi = 0; xi < RozmiarMapy.x && !Break; xi++)
                    {
                        for (int yi = 0; yi < RozmiarMapy.y && !Break; yi++)
                        {
                            if (mapa[xi, yi] == OdwrotnySymbol(symbol))
                            {
                                var atak = NajblizszaKombinacja(xi, yi);
                                if (atak.effective)
                                    ruch1 = (atak.x, atak.y);
                                else
                                    ruch2 = (atak.x, atak.y);
                            }                       
                        }
                    }
                    if (ruch1 != (-1, -1))
                    {
                        x = ruch1.x; 
                        y = ruch1.y;
                    }
                    else if(ruch2 != (-1, -1))
                    {
                        x = ruch2.x;
                        y = ruch2.y;
                    }
                    

                }
                else
                {
                    Random rand = new();
                    Thread.Sleep(600);
                    x = rand.Next(RozmiarMapy.x);
                    y = rand.Next(RozmiarMapy.y);
                    while (mapa[x, y] != Symbol.Pusty)
                    {
                        x = rand.Next(RozmiarMapy.x);
                        y = rand.Next(RozmiarMapy.y);
                    }
                    if (DEBUG_ON)
                    {
                        Debug("random szacujruch=2");
                        Czekaj();
                    }
                }
                if((x == 0 && y == 0 && mapa[x,y] != Symbol.Pusty) || (x == -1 && y == -1))
                {
                    Random rand = new();
                    Thread.Sleep(600);
                    x = rand.Next(RozmiarMapy.x);
                    y = rand.Next(RozmiarMapy.y);
                    while (mapa[x, y] != Symbol.Pusty)
                    {
                        x = rand.Next(RozmiarMapy.x);
                        y = rand.Next(RozmiarMapy.y);
                    }
                    if (DEBUG_ON)
                    {
                        Debug($"random x->{x} y->{y}");
                        Czekaj();
                    }
                }
                Thread.Sleep(600);
                return (x, y);
            }
        }
        private (bool value, (int x, int y) p1, (int x, int y) p2, (int x, int y) p3) CzyPowstalaKombinacja(Symbol symbol, int wiersz, int kolumna)
        {
            if (wiersz > 0 &&                               //pion dla punktu w srodku
                wiersz + 1 < RozmiarMapy.x &&
                mapa[wiersz, kolumna] == symbol &&
                mapa[wiersz - 1, kolumna] == symbol &&
                mapa[wiersz + 1, kolumna] == symbol)
                return (true,
                    (wiersz, kolumna),
                    (wiersz - 1, kolumna),
                    (wiersz + 1, kolumna));
            if (wiersz + 2 < RozmiarMapy.x &&               //pion dla punktu u gory
                mapa[wiersz, kolumna] == symbol &&
                mapa[wiersz + 1, kolumna] == symbol &&
                mapa[wiersz + 2, kolumna] == symbol)
                return (true,
                    (wiersz, kolumna),
                    (wiersz + 1, kolumna),
                    (wiersz + 2, kolumna));
            if (wiersz > 1 &&                               //pion dla punktu na dole
                mapa[wiersz, kolumna] == symbol &&
                mapa[wiersz - 1, kolumna] == symbol &&
                mapa[wiersz - 2, kolumna] == symbol)
                return (true,
                    (wiersz, kolumna),
                    (wiersz - 1, kolumna),
                    (wiersz - 2, kolumna));

            if (kolumna > 0 &&                              //poziom dla punktu w srodku
                kolumna + 1 < RozmiarMapy.y &&
                mapa[wiersz, kolumna] == symbol &&
                mapa[wiersz, kolumna - 1] == symbol &&
                mapa[wiersz, kolumna + 1] == symbol)
                return (true,
                    (wiersz, kolumna),
                    (wiersz, kolumna - 1),
                    (wiersz, kolumna + 1));
            if (kolumna + 2 < RozmiarMapy.y &&              //poziom dla punktu po lewej
                mapa[wiersz, kolumna] == symbol &&
                mapa[wiersz, kolumna + 1] == symbol &&
                mapa[wiersz, kolumna + 2] == symbol)
                return (true,
                    (wiersz, kolumna),
                    (wiersz, kolumna + 1),
                    (wiersz, kolumna + 2));
            if (kolumna > 1 &&                              //poziom dla punktu po prawej
                mapa[wiersz, kolumna] == symbol &&
                mapa[wiersz, kolumna - 1] == symbol &&
                mapa[wiersz, kolumna - 2] == symbol)
                return (true,
                    (wiersz, kolumna),
                    (wiersz, kolumna - 1),
                    (wiersz, kolumna - 2));

            if (kolumna + 2 < RozmiarMapy.y &&              //ukos \ dla punktu w lewym gornym rogu
                wiersz + 2 < RozmiarMapy.x)  
            {
                if (mapa[wiersz, kolumna] == symbol &&
                    mapa[wiersz + 1, kolumna + 1] == symbol &&
                    mapa[wiersz + 2, kolumna + 2] == symbol)
                    return (true,
                        (wiersz, kolumna),
                        (wiersz + 1, kolumna + 1),
                        (wiersz + 2, kolumna + 2));
            }
            if (wiersz > 0 && wiersz + 1 < RozmiarMapy.x &&
                kolumna > 0 && kolumna + 1 < RozmiarMapy.y) //ukos \ dla punktu w srodku
            {
                if (mapa[wiersz, kolumna] == symbol &&
                    mapa[wiersz - 1, kolumna - 1] == symbol &&
                    mapa[wiersz + 1, kolumna + 1] == symbol)
                    return (true,
                        (wiersz, kolumna),
                        (wiersz - 1, kolumna - 1),
                        (wiersz + 1, kolumna + 1));
            }
            if (wiersz > 1 && kolumna > 1)                  //ukos \ dla punktu w prawym dolnym rogu
            {
                if (mapa[wiersz, kolumna] == symbol &&
                    mapa[wiersz - 1, kolumna - 1] == symbol &&
                    mapa[wiersz - 2, kolumna - 2] == symbol)
                    return (true,
                        (wiersz, kolumna),
                        (wiersz - 1, kolumna - 1),
                        (wiersz - 2, kolumna - 2));
            }

            if (wiersz + 2 < RozmiarMapy.x && kolumna > 1)  //ukos / dla punktu w prawym gornym rogu
            {
                if (mapa[wiersz, kolumna] == symbol &&
                    mapa[wiersz + 1, kolumna - 1] == symbol &&
                    mapa[wiersz + 2, kolumna - 2] == symbol)
                    return (true,
                        (wiersz, kolumna),
                        (wiersz + 1, kolumna - 1),
                        (wiersz + 2, kolumna - 2));
            }
            if (wiersz > 0 && wiersz + 1 < RozmiarMapy.x && //ukos / dla punktu w srodku
                kolumna > 0 && kolumna + 1 < RozmiarMapy.y) 
            {
                if (mapa[wiersz, kolumna] == symbol &&
                    mapa[wiersz - 1, kolumna + 1] == symbol &&
                    mapa[wiersz + 1, kolumna - 1] == symbol)
                    return (true,
                        (wiersz, kolumna),
                        (wiersz - 1, kolumna + 1),
                        (wiersz + 1, kolumna - 1));
            }
            if (wiersz > 1 && kolumna + 2 < RozmiarMapy.y)  //ukos / dla punktu w lewym dolnym rogu
            {
                if (mapa[wiersz, kolumna] == symbol &&
                    mapa[wiersz - 1, kolumna + 1] == symbol &&
                    mapa[wiersz - 2, kolumna + 2] == symbol)
                    return (true,
                        (wiersz, kolumna),
                        (wiersz - 1, kolumna + 1),
                        (wiersz - 2, kolumna + 2));
            }
            return (false, (0, 0), (0, 0), (0, 0));
        }

        private bool CzyKoniec()
        {
            for(int x = 0; x < RozmiarMapy.x; x++)
            {
                for(int y = 0; y < RozmiarMapy.y; y++)
                {
                    if (mapa[x, y] == Symbol.Pusty) return false;
                }
            }
            return true;
        } 

        private (int wiersz, int kolumna) PustePole()
        {
            for(int x = 0; x < RozmiarMapy.x; x++)
            {
                for(int y = 0; y < RozmiarMapy.y; y++)
                {
                    if (mapa[x, y] == Symbol.Pusty) return (x, y);
                }
            }
            return (0, 0);
        }

        private (int x, int y) WybierzPole()
        {
            ConsoleKey key;
            (int wiersz, int kolumna) lokalizacja_nowa = PustePole();
            (int wiersz, int kolumna) lokalizacja_poprzednia = lokalizacja_nowa;
            while(true)
            {
                switch(mapa[lokalizacja_poprzednia.wiersz, lokalizacja_poprzednia.kolumna])
                {
                    case Symbol.PustySelected:
                        mapa[lokalizacja_poprzednia.wiersz, lokalizacja_poprzednia.kolumna] = Symbol.Pusty;
                        break;
                    case Symbol.KrzyzykSelected:
                        mapa[lokalizacja_poprzednia.wiersz, lokalizacja_poprzednia.kolumna] = Symbol.Krzyzyk;
                        break;
                    case Symbol.KolkoSelected:
                        mapa[lokalizacja_poprzednia.wiersz, lokalizacja_poprzednia.kolumna] = Symbol.Kolko;
                        break;
                    case Symbol.KrzyzykSolidSelected:
                        mapa[lokalizacja_poprzednia.wiersz, lokalizacja_poprzednia.kolumna] = Symbol.KrzyzykSolid;
                        break;
                    case Symbol.KolkoSolidSelected:
                        mapa[lokalizacja_poprzednia.wiersz, lokalizacja_poprzednia.kolumna] = Symbol.KolkoSolid;
                        break;
                }

                switch(mapa[lokalizacja_nowa.wiersz, lokalizacja_nowa.kolumna])
                {
                    case Symbol.Pusty:
                        mapa[lokalizacja_nowa.wiersz, lokalizacja_nowa.kolumna] = Symbol.PustySelected;
                        break;
                    case Symbol.Krzyzyk:
                        mapa[lokalizacja_nowa.wiersz, lokalizacja_nowa.kolumna] = Symbol.KrzyzykSelected;
                        break;
                    case Symbol.Kolko:
                        mapa[lokalizacja_nowa.wiersz, lokalizacja_nowa.kolumna] = Symbol.KolkoSelected;
                        break;
                    case Symbol.KrzyzykSolid:
                        mapa[lokalizacja_nowa.wiersz, lokalizacja_nowa.kolumna] = Symbol.KrzyzykSolidSelected;
                        break;
                    case Symbol.KolkoSolid:
                        mapa[lokalizacja_nowa.wiersz, lokalizacja_nowa.kolumna] = Symbol.KolkoSolidSelected;
                        break;
                }

                Rysuj(true);
                key = Console.ReadKey().Key;

                if (key == ConsoleKey.LeftArrow)
                {
                    if (lokalizacja_nowa.kolumna - 1 >= 0)
                    {
                        lokalizacja_poprzednia = lokalizacja_nowa;
                        int temp = lokalizacja_nowa.kolumna;
                        lokalizacja_nowa.kolumna--;
                        if (lokalizacja_nowa.kolumna < 0)
                            lokalizacja_nowa.kolumna = temp;
                    }
                }
                else if (key == ConsoleKey.RightArrow)
                {
                    if (lokalizacja_nowa.kolumna + 1 < RozmiarMapy.y)
                    {
                        lokalizacja_poprzednia = lokalizacja_nowa;
                        int temp = lokalizacja_nowa.kolumna;
                        lokalizacja_nowa.kolumna++;
                        if (lokalizacja_nowa.kolumna == RozmiarMapy.y) lokalizacja_nowa.kolumna = temp;
                    }
                }
                else if (key == ConsoleKey.UpArrow)
                {
                    if (lokalizacja_nowa.wiersz - 1 >= 0)
                    {
                        lokalizacja_poprzednia = lokalizacja_nowa;
                        int temp = lokalizacja_nowa.wiersz;
                        lokalizacja_nowa.wiersz--;
                        if (lokalizacja_nowa.wiersz < 0)
                            lokalizacja_nowa.wiersz = temp;
                    }
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    if (lokalizacja_nowa.wiersz + 1 < RozmiarMapy.x)
                    {
                        lokalizacja_poprzednia = lokalizacja_nowa;
                        int temp = lokalizacja_nowa.wiersz;
                        lokalizacja_nowa.wiersz++;
                        if (lokalizacja_nowa.wiersz == RozmiarMapy.x) lokalizacja_nowa.wiersz = temp;
                    }
                }
                else if (key == ConsoleKey.Enter)
                {
                    if (mapa[lokalizacja_nowa.wiersz, lokalizacja_nowa.kolumna] == Symbol.PustySelected) break;
                }
            }
            switch (mapa[lokalizacja_nowa.wiersz, lokalizacja_nowa.kolumna])
            {
                case Symbol.PustySelected:
                    mapa[lokalizacja_nowa.wiersz, lokalizacja_nowa.kolumna] = Symbol.Pusty;
                    break;
                case Symbol.KrzyzykSelected:
                    mapa[lokalizacja_nowa.wiersz, lokalizacja_nowa.kolumna] = Symbol.Krzyzyk;
                    break;
                case Symbol.KolkoSelected:
                    mapa[lokalizacja_nowa.wiersz, lokalizacja_nowa.kolumna] = Symbol.Kolko;
                    break;
                case Symbol.KolkoSolidSelected:
                    mapa[lokalizacja_nowa.wiersz, lokalizacja_nowa.kolumna] = Symbol.KolkoSolid;
                    break;
                case Symbol.KrzyzykSolidSelected:
                    mapa[lokalizacja_nowa.wiersz, lokalizacja_nowa.kolumna] = Symbol.KrzyzykSolid;
                    break;
            }

            return lokalizacja_nowa;
        }

        

        private void Graj()
        {
            score = (0, 0);
            while(true)
            {
                Console.Clear();
                Rysuj(true);
                Print("\n\n");

                (int wiersz, int kolumna) = (aktualny_gracz.mode==Gracz.Mode.Auto) ? bot.WybierzPole(aktualny_gracz.symbol) : WybierzPole();
                mapa[wiersz, kolumna] = aktualny_gracz.symbol;
                (bool value, (int x, int y) p1, (int x, int y) p2, (int x, int y) p3) test;
                if ((test = CzyPowstalaKombinacja(aktualny_gracz.symbol, wiersz, kolumna)).value) {
                    if (mapa[test.p1.x, test.p1.y] == Symbol.Kolko) mapa[test.p1.x, test.p1.y] = Symbol.KolkoSolid;
                    else if (mapa[test.p1.x, test.p1.y] == Symbol.Krzyzyk) mapa[test.p1.x, test.p1.y] = Symbol.KrzyzykSolid;
                    if (mapa[test.p2.x, test.p2.y] == Symbol.Kolko) mapa[test.p2.x, test.p2.y] = Symbol.KolkoSolid;
                    else if (mapa[test.p2.x, test.p2.y] == Symbol.Krzyzyk) mapa[test.p2.x, test.p2.y] = Symbol.KrzyzykSolid;
                    if (mapa[test.p3.x, test.p3.y] == Symbol.Kolko) mapa[test.p3.x, test.p3.y] = Symbol.KolkoSolid;
                    else if (mapa[test.p3.x, test.p3.y] == Symbol.Krzyzyk) mapa[test.p3.x, test.p3.y] = Symbol.KrzyzykSolid;
                    if (aktualny_gracz.symbol == Symbol.Kolko) score.kolko+=3;
                    else if (aktualny_gracz.symbol == Symbol.Krzyzyk) score.krzyzyk += 3;
                    //break;
                }

                if (aktualny_gracz.symbol == gracze.pierwszy.symbol) aktualny_gracz = gracze.drugi;
                else aktualny_gracz = gracze.pierwszy;

                if (CzyKoniec())
                {
                    if (score.kolko > score.krzyzyk)
                    {
                        if (gracze.pierwszy.symbol == Symbol.Kolko)
                            winner = gracze.pierwszy;
                        else
                            winner = gracze.drugi;
                    }
                    else if (score.krzyzyk > score.kolko)
                    {
                        if (gracze.pierwszy.symbol == Symbol.Krzyzyk)
                            winner = gracze.pierwszy;
                        else
                            winner = gracze.drugi;
                    }
                    else
                        winner = new() { mode = Gracz.Mode.Manual, symbol = Symbol.Pusty };
                    break;
                }
            }

            Console.Clear();
            Rysuj(false);

        }

        public void Rysuj(bool czyTrwaRozgrywka)
        {
            Console.CursorVisible = false;

            Console.SetCursorPosition(0, 0);
            if(czyTrwaRozgrywka)
            {
                Console.Write("Teraz gra ");
                if (aktualny_gracz.symbol == Symbol.Kolko)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("Kółko\t\t");
                    Console.ResetColor();
                }
                else if (aktualny_gracz.symbol == Symbol.Krzyzyk)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Krzyżyk\t");
                    Console.ResetColor();
                }
                Console.Write($" Punkty: {score.kolko} O\t{score.krzyzyk} X");
            } 
            else
            {
                if (winner.symbol == Symbol.Kolko)
                {
                    Console.Write("Wygrywa ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("Kółko");
                    Console.ResetColor();
                    Console.Write($" zdobywając {score.kolko} punktów!");
                }
                else if (winner.symbol == Symbol.Krzyzyk)
                {
                    Console.Write("Wygrywa ");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Krzyżyk");
                    Console.ResetColor();
                    Console.Write($" zdobywając {score.krzyzyk} punktów!");
                }
                else if(winner.symbol == Symbol.Pusty)
                {
                    int wynik = (score.kolko == score.krzyzyk) ? score.kolko : -1; //w razie dziwnego błędu wynik wyniesie -1
                    Console.Write($"Remis! Gracze uzyskali równo po {wynik} punktów!");
                }
            }
            
            int temp_x = 1, temp_y;
            for(int x = 0; x < RozmiarMapy.x; x++)
            {
                Console.SetCursorPosition(0, temp_x);
                Console.Write(RepeatString("+-", RozmiarMapy.y) + '+');
                temp_x += 2;
                temp_y = 0;
                for (int y = 0; y < RozmiarMapy.y; y++)
                {
                    Console.SetCursorPosition(temp_y, temp_x-1);
                    Console.Write('|');
                    temp_y += 2;
                    switch (mapa[x, y])
                    {
                        case Symbol.Kolko:
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write('O');
                            Console.ResetColor();
                            break;
                        case Symbol.KolkoSelected:
                            Console.BackgroundColor = ConsoleColor.Gray;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write('O');
                            Console.ResetColor();
                            break;
                        case Symbol.KolkoSolid:
                            Console.BackgroundColor = ConsoleColor.Green;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write('O');
                            Console.ResetColor();
                            break;
                        case Symbol.KolkoSolidSelected:
                            Console.BackgroundColor = ConsoleColor.Gray;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write('O');
                            Console.ResetColor();
                            break;

                        case Symbol.Krzyzyk:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write('X');
                            Console.ResetColor();
                            break;
                        case Symbol.KrzyzykSelected:
                            Console.BackgroundColor = ConsoleColor.Gray;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write('X');
                            Console.ResetColor();
                            break;
                        case Symbol.KrzyzykSolid:
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write('X');
                            Console.ResetColor();
                            break;
                        case Symbol.KrzyzykSolidSelected:
                            Console.BackgroundColor = ConsoleColor.Gray;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write('X');
                            Console.ResetColor();
                            break;

                        case Symbol.Pusty:
                            Console.Write(' ');
                            break;
                        case Symbol.PustySelected:
                            Console.BackgroundColor = ConsoleColor.DarkGray;
                            Console.Write(' ');
                            Console.ResetColor();
                            break;

                    }
                }
                Console.Write('|');
                if (!(x + 1 < RozmiarMapy.x)) //ostatnia linia mapy
                {
                    Console.SetCursorPosition(0, temp_x);
                    Console.Write(RepeatString("+-", RozmiarMapy.y) + '+');
                }
            }
        }

        public void Start()
        {
            Graj();
        }
    }

    internal class Program
    {
        public static void Main() 
        {
            Engine silnik;
            try
            {
                silnik = new(3, 3, true, true, 0);
            }
            catch(Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(e.Message);
                Console.ResetColor();
                return;
            }
            silnik.Start();
        }
    }
}