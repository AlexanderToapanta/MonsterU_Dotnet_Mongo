using System;
using System.Web.UI;

namespace Monster_University
{
    public partial class monsteruniversity : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                VerificarSesionUsuario();
            }
        }

        private void VerificarSesionUsuario()
        {
            if (Session["UsuarioNombre"] != null)
            {
                
                divUserMenu.Visible = true;
                lnkLogin.Visible = false;

                
                lblUserName.Text = Session["UsuarioNombre"].ToString();
                lblUserFullName.Text = Session["UsuarioNombre"].ToString();

                
                if (Session["UsuarioRol"] != null)
                {
                    lblUserRole.Text = Session["UsuarioRol"].ToString();
                }
                else
                {
                    lblUserRole.Text = "Usuario";
                }
            }
            else
            {
                
                divUserMenu.Visible = false;
                lnkLogin.Visible = true;
            }
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
           
            Session.Clear();
            Session.Abandon();

            
            Response.Redirect("~/Login.aspx");
        }
    }
}