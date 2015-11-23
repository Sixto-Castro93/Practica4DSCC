using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;

//Estas referencias son necesarias para usar GLIDE
using GHI.Glide;
using GHI.Glide.Display;
using GHI.Glide.UI;

namespace Practica4DSCC
{
    public partial class Program
    {
        //Objetos de interface gráfica GLIDE
        private GHI.Glide.Display.Window iniciarWindow;
        private GHI.Glide.Display.Window pantallaTemperatura;
        
        private Button btn_inicio;
        private Button atras;
        private ProgressBar barra;
        String ip = "";
        private TextBlock texto;
        private TextBlock textotemp;
        String respuesta = "";
        
        GT.Timer timer = new GT.Timer(15000); // every second (1000ms)
        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            /*******************************************************************************************
            Modules added in the Program.gadgeteer designer view are used by typing 
            their name followed by a period, e.g.  button.  or  camera.
            
            Many modules generate useful events. Type +=<tab><tab> to add a handler to an event, e.g.:
                button.ButtonPressed +=<tab><tab>
            
            If you want to do something periodically, use a GT.Timer and handle its Tick event, e.g.:
                GT.Timer timer = new GT.Timer(1000); // every second (1000ms)
                timer.Tick +=<tab><tab>
                timer.Start();
            *******************************************************************************************/


            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
            ethernetJ11D.NetworkInterface.Open();
            ethernetJ11D.NetworkInterface.EnableDhcp();
            ethernetJ11D.UseThisNetworkInterface();
            //Carga la ventana principal
            iniciarWindow = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.inicioWindow));
            pantallaTemperatura = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.pantallaTemperatura));
            GlideTouch.Initialize();

            //Inicializa el boton en la interface
            btn_inicio = (Button)iniciarWindow.GetChildByName("button_iniciar");
            atras = (Button)pantallaTemperatura.GetChildByName("atras");
            barra = (ProgressBar)pantallaTemperatura.GetChildByName("instance23061");
            texto = (TextBlock)iniciarWindow.GetChildByName("text_net_status");
            textotemp = (TextBlock)pantallaTemperatura.GetChildByName("valor");
            btn_inicio.TapEvent += btn_inicio_TapEvent;
            atras.TapEvent += atras_TapEvent;
            ethernetJ11D.NetworkDown += ethernetJ11D_NetworkDown;
            ethernetJ11D.NetworkUp += ethernetJ11D_NetworkUp;
            timer.Tick += timer_Tick;
            
            //Selecciona iniciarWindow como la ventana de inicio
            Glide.MainWindow = iniciarWindow;
        }

        void atras_TapEvent(object sender)
        {
            timer.Stop();
            Glide.MainWindow = iniciarWindow;
            textotemp.Text = ":(";
            barra.Value = 0;
           
        }

  
        void timer_Tick(GT.Timer timer)
        {
            HttpRequest request = HttpHelper.CreateHttpGetRequest("http://api.thingspeak.com/channels/46434/fields/2/last");
            request.ResponseReceived += request_ResponseReceived;            request.SendRequest();
        }
        void request_ResponseReceived(HttpRequest sender, HttpResponse response)
        {
            respuesta = response.Text;
            Debug.Print(respuesta);
            // btn_inicio.Text = respuesta;
            textotemp.Text = respuesta;
            barra.Value = (int)Double.Parse(respuesta);
            Glide.MainWindow = pantallaTemperatura;
        }


        void ethernetJ11D_NetworkUp(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            btn_inicio.Enabled = true;
   
            ip=ethernetJ11D.NetworkSettings.IPAddress;
            texto.Text = ip;
            Debug.Print("Conectado");
            Glide.MainWindow = iniciarWindow;
        }

        void ethernetJ11D_NetworkDown(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            Debug.Print("Desconectado");
            btn_inicio.Text = "Iniciar";
            btn_inicio.Enabled = false;
            texto.Text = "No Network";
            timer.Stop();
            Glide.MainWindow = iniciarWindow;
           
        }

        void btn_inicio_TapEvent(object sender)
        {
           
            timer.Start();
            Debug.Print("Iniciar");
            Glide.MainWindow = pantallaTemperatura;
           
            
        }
    }
}
