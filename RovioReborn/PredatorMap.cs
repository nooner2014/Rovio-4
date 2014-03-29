using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PredatorPreyAssignment;

namespace Rovio
{
    class PredatorMap : BaseArena
    {
        public PredatorMap(string address, string user, string password, Map m, Object k)
            : base(address, user, password, m, k)
        {
           // map = m;
            
        }

        public override void Start()
        {
            //System.Threading.Thread getImage = new System.Threading.Thread(GetImage);
            //getImage.Start();

            System.Threading.Thread search = new System.Threading.Thread(SearchImage);
            search.Start();

            // System.Threading.Thread source = new System.Threading.Thread(ImageGet);
            //source.Start();

            //System.Threading.Thread move = new System.Threading.Thread(SetFSMAction);
            System.Threading.Thread move = new System.Threading.Thread(InitialMovements);
            //move.Start();



            System.Threading.Thread myT = new System.Threading.Thread(GetNewCorrectDirection);
            myT.Start();

            System.Threading.Thread distance = new System.Threading.Thread(() => FindALLDISTANCE(ref wallDist));
            distance.Start();

            while (running && connected)
            {
                //wallLineHeight = 0;
                //if (preyRectangle.Height != 0)
                //Console.WriteLine((float)25/preyRectangle.Height);

                //wallLineHeight -= 0.2f;

                System.Threading.Thread.Sleep(1000);
                //lock (commandLock)
                //lock (mapLock)
                //if (!(trackingState == Tracking.Initial))
                //Rotate90(1, 6);

                
                //FindDirection();

            }
        }
    }
}
