<%@ Page Title="Inicio - Sistema de Matrículas" Language="C#" MasterPageFile="~/monsteruniversity.master" 
    AutoEventWireup="true" CodeBehind="index.aspx.cs" 
    Inherits="Monster_University.Index" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Inicio - Sistema de Matrículas Monster
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .main-background {
            position: relative;
            min-height: 100vh;
            background-image: url('https://www.bangor.ac.uk/sites/default/files/styles/16x9_1100w/public/2020-05/CB%202018%20May%20%2837%29.jpg?h=41ef3158&amp;itok=zGs3cG5Q');
            background-size: cover;
            background-position: center;
            background-attachment: fixed;
            background-repeat: no-repeat;
        }
        
        .background-overlay {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0, 0, 0, 0.4);
            z-index: 0;
        }
        
        .dashboard-container {
            position: relative;
            z-index: 1;
            max-width: 1200px;
            margin: 0 auto;
            padding: 40px 20px;
        }
        
        .welcome-section h2 {
            color: #2c3e50;
            margin-bottom: 10px;
            font-size: 24px;
        }
        
        .welcome-section p {
            color: #7f8c8d;
            font-size: 16px;
            line-height: 1.6;
        }
        
        .quick-actions {
            display: flex;
            justify-content: center;
            flex-wrap: wrap;
            gap: 15px;
        }
        
        .quick-action-btn {
            padding: 12px 24px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border: none;
            color: white;
            border-radius: 8px;
            font-weight: 600;
            text-decoration: none;
            transition: all 0.3s ease;
            box-shadow: 0 4px 15px rgba(102, 126, 234, 0.3);
        }
        
        .quick-action-btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(102, 126, 234, 0.4);
            color: white;
            text-decoration: none;
        }
        
        .user-info {
            border-left: 4px solid #667eea;
            background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
            margin-top: 20px;
            padding: 15px;
            border-radius: 5px;
        }
        
        .user-info h3 {
            color: #2c3e50;
            margin-bottom: 15px;
            font-size: 18px;
        }
        
        .panel-custom {
            border-radius: 10px;
            box-shadow: 0 5px 20px rgba(0,0,0,0.1);
            border: none;
            margin-bottom: 20px;
            background: rgba(255,255,255,0.95);
        }
        
        .panel-header-custom {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border: none;
            border-radius: 10px 10px 0 0;
            font-weight: 600;
            padding: 15px;
        }
        
        .panel-body-custom {
            padding: 20px;
        }
        
        .info-grid {
            width: 100%;
            margin-top: 10px;
        }
        
        .info-grid td {
            padding: 10px;
            border-bottom: 1px solid #e9ecef;
        }
        
        .info-grid td:first-child {
            font-weight: 600;
            color: #2c3e50;
        }
        
        .status-active {
            color: green;
            font-weight: bold;
        }
        
        .status-inactive {
            color: red;
            font-weight: bold;
        }
        
        .accordion-custom {
            margin-top: 15px;
        }
        
        .accordion-header {
            background-color: #f8f9fa;
            border: 1px solid #dee2e6;
            padding: 10px 15px;
            cursor: pointer;
            font-weight: 600;
            color: #2c3e50;
        }
        
        .accordion-content {
            padding: 15px;
            border: 1px solid #dee2e6;
            border-top: none;
            background-color: white;
        }
        
        .system-footer {
            text-align: center;
            width: 100%;
            position: relative;
            z-index: 2;
            background: rgba(255,255,255,0.9);
            padding: 20px;
            margin-top: 30px;
            border-radius: 10px;
        }
        
        .system-footer p {
            color: #2c3e50;
            font-weight: 600;
            margin: 0;
        }
        
        @media (max-width: 768px) {
            .quick-actions {
                flex-direction: column;
                align-items: center;
            }
            
            .quick-action-btn {
                width: 100%;
                max-width: 250px;
                margin: 5px 0;
            }
            
            .dashboard-container {
                padding: 20px 15px;
            }
            
            .main-background {
                background-attachment: scroll;
            }
            
            .info-grid {
                font-size: 14px;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <div class="main-background">
        <div class="background-overlay"></div>
        <div class="dashboard-container">
            <!-- Tarjeta de Bienvenida -->
            <div class="panel-custom">
                <div class="panel-header-custom">
                    Bienvenido al Sistema
                </div>
                <div class="panel-body-custom">
                    <div class="welcome-section">
                        <h2>Hola, <asp:Label ID="lblUsuarioNombre" runat="server" Text="Usuario"></asp:Label></h2>
                        <p>Bienvenido al Sistema de Gestión de Matrículas Monster</p>
                        
                        <div class="user-info">
                            <h3>Información de tu cuenta:</h3>
                            <table class="info-grid">
                                <tr>
                                    <td>Usuario:</td>
                                    <td><asp:Label ID="lblXeusuNombre" runat="server" Font-Bold="true"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td>Estado:</td>
                                    <td>
                                        <asp:Label ID="lblXeusuEstado" runat="server" Font-Bold="true">
                                            <asp:Label ID="lblEstadoTexto" runat="server"></asp:Label>
                                        </asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td>ID Usuario:</td>
                                    <td><asp:Label ID="lblXeusuId" runat="server" Font-Bold="true"></asp:Label></td>
                                </tr>
                            </table>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Accesos Rápidos -->
            <div class="panel-custom">
                <div class="panel-header-custom">
                    Accesos Rápidos
                </div>
                <div class="panel-body-custom" style="text-align: center;">
                    <div class="quick-actions">
                        <asp:HyperLink ID="lnkCambiarContrasena" runat="server" 
                            NavigateUrl="~/CambiarContrasena.aspx"
                            CssClass="quick-action-btn">
                            <i class="fas fa-key me-2"></i>Cambiar Contraseña
                        </asp:HyperLink>
                        
                        <asp:HyperLink ID="lnkMiPerfil" runat="server" 
                            NavigateUrl="~/MiPerfil.aspx"
                            CssClass="quick-action-btn">
                            <i class="fas fa-user-edit me-2"></i>Mi Perfil
                        </asp:HyperLink>
                        
                        <asp:HyperLink ID="lnkReportes" runat="server" 
                            NavigateUrl="~/Reporte.aspx"
                            CssClass="quick-action-btn">
                            <i class="fas fa-chart-bar me-2"></i>Reportes
                        </asp:HyperLink>
                    </div>
                </div>
            </div>

            <!-- Información del Sistema -->
            <div class="panel-custom">
                <div class="panel-header-custom">
                    Información del Sistema
                </div>
                <div class="panel-body-custom">
                    <div class="accordion-custom" id="accordionSystem">
                        <div class="accordion-header" data-bs-toggle="collapse" data-bs-target="#collapseOne">
                            Acerca del Sistema
                        </div>
                        <div id="collapseOne" class="accordion-content collapse show">
                            <p>
                                <strong>Sistema de Gestión de Matrículas Monster</strong><br/>
                                Versión: 1.0<br/>
                                Desarrollado para la gestión integral de matrículas, estudiantes, asignaturas y docentes.
                            </p>
                        </div>
                        
                        <div class="accordion-header" data-bs-toggle="collapse" data-bs-target="#collapseTwo">
                            Últimas Actividades
                        </div>
                        <div id="collapseTwo" class="accordion-content collapse">
                            <p>No hay actividades recientes.</p>
                        </div>
                        
                        
                        
                    </div>
                </div>
            </div>
            
            <!-- Footer del sistema -->
            <div class="system-footer">
                <p>Sistema de Matrículas Monster - © <%: DateTime.Now.Year %></p>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script>
        $(document).ready(function() {
           
            $('.accordion-header').click(function() {
                $(this).toggleClass('active');
            });
            
           
            var estado = $('#<%= lblEstadoTexto.ClientID %>').text();
            if (estado !== 'ACTIVO') {
                $('#collapseThree').collapse('hide');
            }
        });
    </script>
</asp:Content>