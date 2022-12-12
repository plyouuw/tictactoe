using System;
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
        private Menu.Theme styl_menu = Menu.Theme.RedArrow;
        private (Gracz pierwszy, Gracz drugi) gracze;
        private Gracz aktualny_gracz, winner;
        private (int kolko, int krzyzyk) score;
        public Engine(int x_map, int y_map, int opoznienie_ai = 500, bool czy_pierwszy_gracz_to_komputer = true, bool czy_drugi_gracz_to_komputer = true)
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
                RozmiarMapy = rozmiarMapy;
                Delay = delay;
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
                Thread.Sleep(Delay);
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

        private (int x, int y) WybierzPole()
        {
            ConsoleKey key;

            (int wiersz, int kolumna) lokalizacja_nowa = (0, 0);
            for (int x = 0; x < RozmiarMapy.x; x++)
            {
                for (int y = 0; y < RozmiarMapy.y; y++)
                {
                    if (mapa[x, y] == Symbol.Pusty) lokalizacja_nowa = (x, y);
                }
            }

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
            for (int x = 0; x < RozmiarMapy.x; x++)
            {
                for (int y = 0; y < RozmiarMapy.y; y++)
                {
                    mapa[x, y] = Symbol.Pusty;

                }
            }
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

        private void Rysuj(bool czyTrwaRozgrywka)
        {
            Console.CursorVisible = false;
            
            Console.SetCursorPosition(0, 0);
            if (aktualny_gracz.symbol == Symbol.Kolko && czyTrwaRozgrywka)
            {
                Print("Teraz gra");
                Print("     Kółko ", ConsoleColor.Green);
            }
            else if (aktualny_gracz.symbol == Symbol.Krzyzyk && czyTrwaRozgrywka)
            {
                Print("Teraz gra");
                Print("    Krzyżyk", ConsoleColor.Red);
            }
            else if (!czyTrwaRozgrywka) Print("Koniec gry!\t");
            Console.Write($"\tKółko: {score.kolko} pkt\tKrzyżyk: {score.krzyzyk} pkt");
            
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
                    Print('|');
                    temp_y += 2;
                    switch (mapa[x, y])
                    {
                        case Symbol.Kolko:
                            Print('O', ConsoleColor.Green);
                            break;
                        case Symbol.KolkoSelected:
                            Print('O', ConsoleColor.Green, ConsoleColor.Gray);
                            break;
                        case Symbol.KolkoSolid:
                            Print('O', ConsoleColor.White, ConsoleColor.Green);
                            break;
                        case Symbol.KolkoSolidSelected:
                            Print('O', ConsoleColor.Green, ConsoleColor.Gray);
                            break;

                        case Symbol.Krzyzyk:
                            Print('X', ConsoleColor.Red);
                            break;
                        case Symbol.KrzyzykSelected:
                            Print('X', ConsoleColor.Red, ConsoleColor.Gray);
                            break;
                        case Symbol.KrzyzykSolid:
                            Print('X', ConsoleColor.White, ConsoleColor.Red);
                            break;
                        case Symbol.KrzyzykSolidSelected:
                            Print('X', ConsoleColor.Red, ConsoleColor.Gray);
                            break;

                        case Symbol.Pusty:
                            Print(' ');
                            break;
                        case Symbol.PustySelected:
                            Print(' ', ConsoleColor.Black, ConsoleColor.DarkGray);
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
            if(!czyTrwaRozgrywka)
            {
                Print("\n");
                if (winner.symbol == Symbol.Kolko)
                {
                    Print("Wygrywa ");
                    Print("Kółko", ConsoleColor.Green);
                    Print($" zdobywając {score.kolko} punktów!");
                }
                else if (winner.symbol == Symbol.Krzyzyk)
                {
                    Print("Wygrywa ");
                    Print("Krzyżyk", ConsoleColor.Red);
                    Print($" zdobywając {score.krzyzyk} punktów!");
                }
                else if (winner.symbol == Symbol.Pusty)
                {
                    int wynik = (score.kolko == score.krzyzyk) ? score.kolko : -1; //w razie dziwnego błędu wynik wyniesie -1
                    Print($"Remis! Gracze uzyskali równo po {wynik} punktów!");
                }
                Print("\n\nNaciśnij ENTER, aby rozpocząć kolejną grę. \nInny dowolny przycisk kończy działanie programu.");
                ConsoleKey read = Console.ReadKey().Key;
                if (read == ConsoleKey.Enter) Graj();
                Console.Clear();
            }
        }

        public void Start()
        {
            if(mapa.Length == 0)
            {
                PrintError("Gra nie została zainicjalizowana!");
                return;
            }
            Dictionary<int, string> dict = new()
            {
                { 1, "Manualny" },
                { 2, "Automat" }
            };
            Menu menu = new(styl_menu, dict, "Kółko i krzyżyk\n\nWybierz, tryb gracza Kółko:");
            int choice = menu.ReadChoice();
            if (choice == 1) gracze.pierwszy.mode = Gracz.Mode.Manual;
            else gracze.pierwszy.mode = Gracz.Mode.Auto;

            menu = new(styl_menu, dict, "Kółko i krzyżyk\n\nWybierz, tryb gracza Krzyżyk:");
            choice = menu.ReadChoice();
            if (choice == 1) gracze.drugi.mode = Gracz.Mode.Manual;
            else gracze.drugi.mode = Gracz.Mode.Auto;

            dict = new()
            {
                { 1, "Losowanie gracza, który rozpocznie" },
                { 2, "Rozpocznij od Kółka" },
                { 3, "Rozpocznij od Krzyżyka" }
            };
            menu = new(styl_menu, dict, "Kółko i krzyżyk\n\nWybierz, co chcesz zrobić:");
            choice = menu.ReadChoice();
            if (choice == 1)
            {
                Console.Clear();
                Print("Losowanie polega na wprowadzniu przez obu graczy liczby od 0 do 100.\nGracz, który wprowadzi większą liczbę rozpoczyna rozgrywkę.\n\nWprowadzanie liczb powinno być niejawne dla obu graczy. Wynik losowania pokaże się po zakończeniu wprowadzania liczb.");
                Czekaj();
                while (true)
                {
                    Console.Clear();
                    Print("Gracz Kółko\n");
                    int first = ReadInt(true, "Wprowadź swoją liczbę: ", "Liczba powinna być z zakresu od 0 do 100!", 0, 100);
                    Console.Clear();
                    Print("Gracz Krzyżyk\n");
                    int second = ReadInt(true, "Wprowadź swoją liczbę: ", "Liczba powinna być z zakresu od 0 do 100!", 0, 100);
                    Console.Clear();
                    if (first > second)
                    {
                        Print("Rozpocznie gracz Kółko!");
                        aktualny_gracz = gracze.pierwszy;
                        Czekaj();
                        break;
                    }
                    else if (second > first)
                    {
                        Print("Rozpocznie gracz Krzyżyk!");
                        aktualny_gracz = gracze.drugi;
                        Czekaj();
                        break;
                    }
                    else
                    {
                        Print("Remis! Spróbujcie ponownie!");
                        Czekaj();
                    }
                }
            }
            else if (choice == 2) aktualny_gracz = gracze.pierwszy;
            else aktualny_gracz = gracze.drugi;

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
                silnik = new(3, 3);
            }
            catch(Exception e)
            {
                PrintError(e.Message);
                return;
            }
            silnik.Start();
        }
    }
}