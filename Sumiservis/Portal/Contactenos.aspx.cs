﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;
using System.Net;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Data;

namespace PortalTrabajadores.Portal
{
    public partial class Contactenos : System.Web.UI.Page
    {
        string Cn = ConfigurationManager.ConnectionStrings["trabajadoresConnectionString"].ConnectionString.ToString();
        string bd1 = ConfigurationManager.AppSettings["BD1"].ToString();

        #region Definicion de los Metodos de la Clase

        #region Metodo Page Load
        /* ****************************************************************************/
        /* Metodo que se ejecuta al momento de la carga de la Pagina Maestra
        /* ****************************************************************************/
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["usuario"] == null)
            {
                //Redirecciona a la pagina de login en caso de que el usuario no se halla autenticado
                Response.Redirect("~/Login.aspx");
            }
            else
            {
                if (!IsPostBack)
                {
                    LlenadoDropBox utilLlenar = new LlenadoDropBox();
                    string command = "SELECT idTiposolicitud, Nombre_TipoSolicitud FROM " + bd1 + ".TipoSolicitudes where TipoPortal = 'T' and Activo = 1 order by orden;";
                    DropListTipoSol.Items.Clear();
                    DropListTipoSol.DataSource = utilLlenar.LoadTipoID(command);
                    DropListTipoSol.DataTextField = "Nombre_TipoSolicitud";
                    DropListTipoSol.DataValueField = "idTiposolicitud";
                    DropListTipoSol.DataBind();
                    CnMysql Conexion = new CnMysql(Cn);
                    try
                    {
                        MySqlCommand scSqlCommand = new MySqlCommand("SELECT descripcion FROM " + bd1 + ".Options_Menu WHERE url = 'Contactenos.aspx'", Conexion.ObtenerCnMysql());
                        MySqlDataAdapter sdaSqlDataAdapter = new MySqlDataAdapter(scSqlCommand);
                        DataSet dsDataSet = new DataSet();
                        DataTable dtDataTable = null;
                        Conexion.AbrirCnMysql();
                        sdaSqlDataAdapter.Fill(dsDataSet);
                        dtDataTable = dsDataSet.Tables[0];
                        if (dtDataTable != null && dtDataTable.Rows.Count > 0)
                        {
                            this.lblTitulo.Text = dtDataTable.Rows[0].ItemArray[0].ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        MensajeError("Ha ocurrido el siguiente error: " + ex.Message + " _Metodo: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
                    }
                    finally
                    {
                        Conexion.CerrarCnMysql();
                    }
                }
            }
        }
        #endregion

        #region Metodo BtnEnviar_Click
        /* ****************************************************************************/
        /* Evento del Boton Enviar Correo
        /* ****************************************************************************/
        protected void BtnEnviar_Click(object sender, EventArgs e)
        {
            //string NomBolsaEmpleo = ConfigurationManager.AppSettings["NomBolsaEmpleo"].ToString();
            string ServidorSMTP = ConfigurationManager.AppSettings["ServidorSMTP"].ToString();
            string UsuarioCorreo = ConfigurationManager.AppSettings["UsuarioCorreo"].ToString();
            string ClaveCorreo = ConfigurationManager.AppSettings["ClaveCorreo"].ToString();
            string DestinatarioContactenos = "";

            if (DropListTipoSol.SelectedItem.Text == "Consulta sobre el pago de Seguridad Social")
            {
                DestinatarioContactenos = ConfigurationManager.AppSettings["DestinatariSegSocial"].ToString();
            }
            else if (DropListTipoSol.SelectedItem.Text == "Consulta sobre la liquidación de Nómina")
            {
                DestinatarioContactenos = ConfigurationManager.AppSettings["DestinatarioNomina"].ToString();
            }
            else if (DropListTipoSol.SelectedItem.Text == "Reporte de Perdida de Documentos")
            {
                DestinatarioContactenos = ConfigurationManager.AppSettings["DestinatarioReporte"].ToString();
            }
            else
            {
                DestinatarioContactenos = ConfigurationManager.AppSettings["DestinatarioContactenos"].ToString();
            }

            int port = Convert.ToInt16(ConfigurationManager.AppSettings["port"]);
            string displayName = "";

            //Cambia el texto a mostarar en el nombre del destinatario
            if (TxtMail.Text == "")
            {
                displayName = "Contacto Cliente";
            }
            else
            {
                displayName = TxtMail.Text;
            }

            System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
            msg.To.Add(DestinatarioContactenos);
            msg.From = new MailAddress(UsuarioCorreo, displayName, System.Text.Encoding.UTF8);
            msg.Subject = DropListTipoSol.SelectedItem.Text;
            msg.SubjectEncoding = System.Text.Encoding.UTF8;
            msg.Body = "La siguiente solicitud es realizada por: " + Session["nombre"].ToString() + " identificado(a) con la cedula: " + Session["usuario"].ToString() + Environment.NewLine + "Motivo: " + DropListTipoSol.SelectedItem.Text + Environment.NewLine + "Solicita respuesta al correo: " + TxtMail.Text + Environment.NewLine + "Contenido del mensaje: " + TxtMensaje.Text;
            msg.BodyEncoding = System.Text.Encoding.UTF8;
            msg.IsBodyHtml = false;

            //Cliente que se conecta al servidor SMTP
            SmtpClient client = new SmtpClient();
            client.Credentials = new System.Net.NetworkCredential(UsuarioCorreo, ClaveCorreo);
            client.Port = port;
            client.Host = ServidorSMTP;
            client.EnableSsl = false; //Esto es para que vaya a través de SSL
            try
            {
                client.Send(msg);
                lblMdlPop.Text = "Mensaje enviado Satisfactoriamente";
                ModalPopupExtender1.Show();
                TxtMail.Text = "";
                TxtMensaje.Text = "";
            }
            catch 
            {
                //MensajeError("Ha ocurrido el siguiente error: " + E.Message + " _Metodo: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
                lblMdlPop.Text = "El mensaje no se pudo enviar. Intentelo más Tarde.";
                ModalPopupExtender1.Show();
            }
            finally
            {
                client.Dispose();
            }
        }
        #endregion

        #region Metodo MensajeError
        /* ****************************************************************************/
        /* Metodo que habilita el label de mensaje de error
        /* ****************************************************************************/
        public void MensajeError(string Msj)
        {
            ContentPlaceHolder cPlaceHolder;
            cPlaceHolder = (ContentPlaceHolder)Master.FindControl("Errores");
            if (cPlaceHolder != null)
            {
                Label lblMessage = (Label)cPlaceHolder.FindControl("LblMsj") as Label;
                if (lblMessage != null)
                {
                    lblMessage.Text = Msj;
                    lblMessage.Visible = true;
                }
            }
        }
        #endregion

        #region Metodo btnok_Click
        /* ****************************************************************************/
        /* Evento del Boton de la Ventana Modal al Dar Clic
        /* ****************************************************************************/
        protected void btnok_Click(object sender, EventArgs e)
        {
            TxtMail.Text = "";
            TxtMensaje.Text = "";
        }
        #endregion

        #endregion
    }
}