﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Net.Mobile.Forms;
namespace LIP
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class IgresarProductosPage : ContentPage
	{
        public Entidades.Auth Usuario = new Entidades.Auth();
        DataAccess db = new DataAccess();
        public string CodigoProducto;
        public string NombreProducto;

        public IgresarProductosPage ()
		{
			InitializeComponent ();
         
		}
        public void Cargar() {
            this.btnCodigo.Text = CodigoProducto; 
            this.lblNombre.Text = NombreProducto;
            
        }

        async void btnEscanear_Clicked(object sender, EventArgs e)
        {
            var scann = new ZXingBarcodeImageView();
            var pagina = new ZXingScannerPage();
            pagina.AutoFocus();
            pagina.HasTorch = true;
            pagina.Title = "Escaneando codigo de barra";

            //setup options
            var options = new ZXing.Mobile.MobileBarcodeScanningOptions
            {
                AutoRotate = false,
                UseFrontCameraIfAvailable = true,
                TryHarder = true,
                PossibleFormats = new List<ZXing.BarcodeFormat>
                        {
                           ZXing.BarcodeFormat.EAN_8, ZXing.BarcodeFormat.EAN_13
                        }
                };

             var opciones = new ZXingScannerPage(options);  
            

            await Navigation.PushAsync(pagina);

            pagina.OnScanResult += (resultado) =>
            {
                pagina.IsScanning = false;

                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PopAsync();
                    lblResultado.Text = resultado.Text;
                });
            };
        }

        void btnGuardar_Clicked(object sender, EventArgs e) {

           
                var pro = new Entidades.DetalleLevantadoTemp();
                var Resp = new Entidades.Respuesta();
                Services.ProductosServices Productos = new Services.ProductosServices();
                pro.Codigo_Usuario = Usuario.Codigo_Usuario;
                pro.CodigoSucursal = Usuario.Sucursal;
                pro.Codigo_Ubicacion = Usuario.Codigo_Ubicacion;
                pro.Codigo_Factura = Usuario.Parcial;
                pro.Codigo_Barra = this.lblResultado.Text;
                pro.Codigo_Producto = this.CodigoProducto;
                pro.Bodega = this.Usuario.Bodega;
                pro.Cargado = true;
                pro.Tipo_Origen = 1;

                 if (Usuario.Conteo == 0)
                {
                    pro.Cantidad = double.Parse(this.txtCantidad.Text);
                    pro.Resultado = double.Parse(this.txtCantidad.Text); ;

                }
                if (Usuario.Conteo == 1)
                {
                    pro.Resultado = double.Parse(this.txtCantidad.Text); 
                    pro.Conteo1 = double.Parse(this.txtCantidad.Text); 
                    pro.UC1 = Usuario.Codigo_Usuario;
                }
                if (Usuario.Conteo == 2)
                {
                    pro.Resultado = double.Parse(this.txtCantidad.Text);
                    pro.Conteo2 = double.Parse(this.txtCantidad.Text);
                    pro.UC2 = Usuario.Codigo_Usuario;
                }
                if (Usuario.Conteo == 3)
                {
                    pro.Resultado = double.Parse(this.txtCantidad.Text);
                    pro.Conteo3 = double.Parse(this.txtCantidad.Text);
                    pro.UC3 = Usuario.Codigo_Usuario;
                }

            Device.BeginInvokeOnMainThread(async () =>
            {
                var monto = new Double();
                try 
                {
                   
                    Resp =  Productos.GuardarProducto(pro);
                    if (Resp.Code == 3)
                    {
                        var producto = new Entidades.DetalleLevantadoTemp();
                        producto = JsonConvert.DeserializeObject<Entidades.DetalleLevantadoTemp>(Resp.Objeto.ToString());
                        var respuesta = await DisplayAlert(@"LIP Producto Contado", " ya tiene conteo de " + producto.Resultado + "\n\r"
                                                                 + " que acción desea realizar?." + "\n\r" +
                                                                   " Actualizar = " + pro.Resultado + "\n\r" +
                                                                   " Agregar(Sumar) = " + (pro.Resultado + producto.Resultado), "Agregar", "Actualizar");
                   
                        if (!respuesta)
                        { //true -> Sobreescribir
                            monto = pro.Resultado;
                        }
                        else
                        {// Agregarla
                            monto = (pro.Resultado + producto.Resultado);
                            monto = (float)(Math.Round((double)monto, 2));
                        }

                        if (Usuario.Conteo == 0)
                        {
                            pro.Cantidad = monto;
                            pro.Resultado = monto;

                        }
                        if (Usuario.Conteo == 1)
                        {
                            pro.Resultado = monto;
                            pro.Conteo1 = monto;
                            pro.UC1 = Usuario.Codigo_Usuario;
                        }
                        if (Usuario.Conteo == 2)
                        {
                            pro.Resultado = monto;
                            pro.Conteo2 = monto;
                            pro.UC2 = Usuario.Codigo_Usuario;
                        }
                        if (Usuario.Conteo == 3)
                        {
                            pro.Resultado = monto;
                            pro.Conteo3 = monto;
                            pro.UC3 = Usuario.Codigo_Usuario;
                        }
                        var R =  Productos.ActualizarProducto(pro);
                        Resp.Response = R.Response;
                        Resp.Code = R.Code;
                    }

             
                   

                    if (Resp.Code == 1) {
                        var lista = new List<Entidades.DetalleLevantadoTemp>();
                        lista = db.BuscarProductoDetalle(pro);
                        if (lista.Count > 0)
                        {
                            db.ActualizarProductoDetalle(pro.Codigo_Usuario, pro.Codigo_Producto, pro.Codigo_Factura, pro.CodigoSucursal, pro.Codigo_Ubicacion, pro.Bodega, monto , Usuario.Conteo);
                        }
                        else
                        {
                            db.GuardarProductoDetalle(pro);
                        }
                        await DisplayAlert("LIP", Resp.Response, "Aceptar");
                        await Navigation.PopAsync();
                    }

                    if (Resp.Code == 5) {

                        await DisplayAlert("LIP", Resp.Response, "Aceptar");
                        Usuario.Codigo_Ubicacion = 0;
                        db.UpdateLevantado(Usuario);
                        await Navigation.PopAsync();
                    }


                    if (Resp.Code == 6)
                    {
                        await DisplayAlert("LIP", Resp.Response, "Aceptar");
                        Usuario.Codigo_Ubicacion = 0;
                        Usuario.IsCerrado = true ;
                        db.UpdateLevantado(Usuario);
                        await Navigation.PopAsync();
                    }


                    if (Resp.Response == null) {
                        await DisplayAlert("LIP", "Error Tiempo Espera Excedido!!", "Aceptar");
                    }

                }
                catch (Exception)
                {
                    await DisplayAlert("LIP","Ocurrio un error al guardar!", "Aceptar");
                    return;
                   // throw;
                }
              
            });
  
                //await Navigation.PopAsync();
                //lblResultado.Text = resultado.Text;
           

        }


    }
}