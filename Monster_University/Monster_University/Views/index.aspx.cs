using System;
using System.Web.UI;

namespace Monster_University
{
    public partial class Index : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarInformacionUsuario();
                
            }
        }

        private void CargarInformacionUsuario()
        {
            
            if (Session["UsuarioNombre"] != null)
            {
                string nombreUsuario = Session["UsuarioNombre"].ToString();
                string idUsuario = Session["UsuarioId"] != null ? Session["UsuarioId"].ToString() : "N/A";
                string estadoUsuario = Session["UsuarioEstado"] != null ? Session["UsuarioEstado"].ToString() : "ACTIVO";

               
                lblUsuarioNombre.Text = nombreUsuario;
                lblXeusuNombre.Text = nombreUsuario;
                lblXeusuId.Text = idUsuario;
                lblEstadoTexto.Text = estadoUsuario;

                
                if (estadoUsuario == "ACTIVO")
                {
                    lblEstadoTexto.CssClass = "status-active";
                }
                else
                {
                    lblEstadoTexto.CssClass = "status-inactive";
                }
            }
            else
            {
                
                Response.Redirect("~/Login.aspx");
            }
        }

        
    }
}