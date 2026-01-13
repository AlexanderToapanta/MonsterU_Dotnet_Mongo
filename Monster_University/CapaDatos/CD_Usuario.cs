using CapaModelo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CD_Usuario
    {
        public static CD_Usuario _instancia = null;

        private CD_Usuario()
        {
        }

        public static CD_Usuario Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    _instancia = new CD_Usuario();
                }
                return _instancia;
            }
        }

        // Método de login
        public Tuple<int, string, string> LoginUsuario(string XEUSU_NOMBRE, string XEUSU_CONTRA)
        {
            int respuesta = 0;
            string rolId = "";
            string usuarioId = "";

            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string contrasenaEncriptada = Encriptar(XEUSU_CONTRA);

                    string query = @"SELECT XEUSU_ID, XEROL_ID 
                                   FROM XEUSU_USUAR 
                                   WHERE XEUSU_NOMBRE = @XEUSU_NOMBRE 
                                   AND XEUSU_CONTRA = @XEUSU_CONTRA 
                                   AND XEUSU_ESTADO = 'ACTIVO'";

                    SqlCommand cmd = new SqlCommand(query, oConexion);
                    cmd.Parameters.AddWithValue("@XEUSU_NOMBRE", XEUSU_NOMBRE);
                    cmd.Parameters.AddWithValue("@XEUSU_CONTRA", contrasenaEncriptada);

                    oConexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            respuesta = 1;
                            usuarioId = dr["XEUSU_ID"]?.ToString();
                            rolId = dr["XEROL_ID"]?.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    respuesta = 0;
                    System.Diagnostics.Debug.WriteLine("Error en LoginUsuario: " + ex.Message);
                }
            }
            return Tuple.Create(respuesta, usuarioId, rolId);
        }

        public Usuario ObtenerDetalleUsuario(string XEUSU_ID)
        {
            Usuario rptUsuario = new Usuario();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                string query = @"SELECT XEUSU_ID, XEROL_ID, PEPER_ID, MECARR_ID, MEEST_ID, 
                                        XEUSU_NOMBRE, XEUSU_CONTRA, XEUSU_ESTADO
                                 FROM XEUSU_USUAR 
                                 WHERE XEUSU_ID = @XEUSU_ID";

                SqlCommand cmd = new SqlCommand(query, oConexion);
                cmd.Parameters.AddWithValue("@XEUSU_ID", XEUSU_ID);

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        rptUsuario = new Usuario()
                        {
                            XEUSU_ID = dr["XEUSU_ID"]?.ToString(),
                            XEROL_ID = dr["XEROL_ID"]?.ToString(),
                            PEPER_ID = dr["PEPER_ID"]?.ToString(),
                            MECARR_ID = dr["MECARR_ID"]?.ToString(),
                            MEEST_ID = dr["MEEST_ID"]?.ToString(),
                            XEUSU_NOMBRE = dr["XEUSU_NOMBRE"]?.ToString(),
                            XEUSU_CONTRA = dr["XEUSU_CONTRA"]?.ToString(),
                            XEUSU_ESTADO = dr["XEUSU_ESTADO"]?.ToString()
                        };
                    }
                    else
                    {
                        rptUsuario = null;
                    }
                    dr.Close();
                    return rptUsuario;
                }
                catch (Exception ex)
                {
                    rptUsuario = null;
                    System.Diagnostics.Debug.WriteLine("Error en ObtenerDetalleUsuario: " + ex.Message);
                    return rptUsuario;
                }
            }
        }

        public List<Usuario> ObtenerUsuarios()
        {
            List<Usuario> rptListaUsuario = new List<Usuario>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                string query = @"SELECT XEUSU_ID, XEROL_ID, PEPER_ID, MECARR_ID, MEEST_ID, 
                                        XEUSU_NOMBRE, XEUSU_CONTRA, XEUSU_ESTADO
                                 FROM XEUSU_USUAR 
                                 ORDER BY XEUSU_ID";

                SqlCommand cmd = new SqlCommand(query, oConexion);

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        rptListaUsuario.Add(new Usuario()
                        {
                            XEUSU_ID = dr["XEUSU_ID"]?.ToString(),
                            XEROL_ID = dr["XEROL_ID"]?.ToString(),
                            PEPER_ID = dr["PEPER_ID"]?.ToString(),
                            MECARR_ID = dr["MECARR_ID"]?.ToString(),
                            MEEST_ID = dr["MEEST_ID"]?.ToString(),
                            XEUSU_NOMBRE = dr["XEUSU_NOMBRE"]?.ToString(),
                            XEUSU_CONTRA = dr["XEUSU_CONTRA"]?.ToString(),
                            XEUSU_ESTADO = dr["XEUSU_ESTADO"]?.ToString()
                        });
                    }
                    dr.Close();
                    return rptListaUsuario;
                }
                catch (Exception ex)
                {
                    rptListaUsuario = null;
                    System.Diagnostics.Debug.WriteLine("Error en ObtenerUsuarios: " + ex.Message);
                    return rptListaUsuario;
                }
            }
        }

        public bool RegistrarUsuario(Usuario oUsuario)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string contrasenaEncriptada = Encriptar(oUsuario.XEUSU_CONTRA);

                    string query = @"INSERT INTO XEUSU_USUAR 
                                    (XEUSU_ID, XEROL_ID, PEPER_ID, MECARR_ID, MEEST_ID, 
                                     XEUSU_NOMBRE, XEUSU_CONTRA, XEUSU_ESTADO)
                                     VALUES 
                                    (@XEUSU_ID, @XEROL_ID, @PEPER_ID, @MECARR_ID, @MEEST_ID, 
                                     @XEUSU_NOMBRE, @XEUSU_CONTRA, @XEUSU_ESTADO)";

                    SqlCommand cmd = new SqlCommand(query, oConexion);
                    cmd.CommandType = CommandType.Text;

                    // Parámetros principales
                    cmd.Parameters.AddWithValue("@XEUSU_ID", oUsuario.XEUSU_ID);
                    cmd.Parameters.AddWithValue("@XEUSU_NOMBRE", oUsuario.XEUSU_NOMBRE);
                    cmd.Parameters.AddWithValue("@XEUSU_CONTRA", contrasenaEncriptada);
                    cmd.Parameters.AddWithValue("@XEUSU_ESTADO", oUsuario.XEUSU_ESTADO);

                    // Parámetros FK
                    cmd.Parameters.AddWithValue("@XEROL_ID",
                        string.IsNullOrEmpty(oUsuario.XEROL_ID) ? (object)DBNull.Value : oUsuario.XEROL_ID);
                    cmd.Parameters.AddWithValue("@PEPER_ID",
                        string.IsNullOrEmpty(oUsuario.PEPER_ID) ? (object)DBNull.Value : oUsuario.PEPER_ID);
                    cmd.Parameters.AddWithValue("@MECARR_ID",
                        string.IsNullOrEmpty(oUsuario.MECARR_ID) ? (object)DBNull.Value : oUsuario.MECARR_ID);
                    cmd.Parameters.AddWithValue("@MEEST_ID",
                        string.IsNullOrEmpty(oUsuario.MEEST_ID) ? (object)DBNull.Value : oUsuario.MEEST_ID);

                    oConexion.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    respuesta = rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error en RegistrarUsuario: " + ex.Message);
                }
            }
            return respuesta;
        }

        public bool ModificarUsuario(Usuario oUsuario)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string contrasenaParaGuardar = oUsuario.XEUSU_CONTRA;

                    // Solo encriptar si no parece estar ya encriptada
                    if (!string.IsNullOrEmpty(oUsuario.XEUSU_CONTRA) && oUsuario.XEUSU_CONTRA.Length < 50)
                    {
                        contrasenaParaGuardar = Encriptar(oUsuario.XEUSU_CONTRA);
                    }

                    string query = @"UPDATE XEUSU_USUAR SET 
                                    XEROL_ID = @XEROL_ID,
                                    PEPER_ID = @PEPER_ID,
                                    MECARR_ID = @MECARR_ID,
                                    MEEST_ID = @MEEST_ID,
                                    XEUSU_NOMBRE = @XEUSU_NOMBRE,
                                    XEUSU_CONTRA = @XEUSU_CONTRA,
                                    XEUSU_ESTADO = @XEUSU_ESTADO
                                    WHERE XEUSU_ID = @XEUSU_ID";

                    SqlCommand cmd = new SqlCommand(query, oConexion);

                    // Parámetros principales
                    cmd.Parameters.AddWithValue("@XEUSU_ID", oUsuario.XEUSU_ID);
                    cmd.Parameters.AddWithValue("@XEUSU_NOMBRE", oUsuario.XEUSU_NOMBRE);
                    cmd.Parameters.AddWithValue("@XEUSU_CONTRA", contrasenaParaGuardar);
                    cmd.Parameters.AddWithValue("@XEUSU_ESTADO", oUsuario.XEUSU_ESTADO);

                    // Parámetros FK
                    cmd.Parameters.AddWithValue("@XEROL_ID",
                        string.IsNullOrEmpty(oUsuario.XEROL_ID) ? (object)DBNull.Value : oUsuario.XEROL_ID);
                    cmd.Parameters.AddWithValue("@PEPER_ID",
                        string.IsNullOrEmpty(oUsuario.PEPER_ID) ? (object)DBNull.Value : oUsuario.PEPER_ID);
                    cmd.Parameters.AddWithValue("@MECARR_ID",
                        string.IsNullOrEmpty(oUsuario.MECARR_ID) ? (object)DBNull.Value : oUsuario.MECARR_ID);
                    cmd.Parameters.AddWithValue("@MEEST_ID",
                        string.IsNullOrEmpty(oUsuario.MEEST_ID) ? (object)DBNull.Value : oUsuario.MEEST_ID);

                    oConexion.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    respuesta = rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error en ModificarUsuario: " + ex.Message);
                }
            }
            return respuesta;
        }

        public bool EliminarUsuario(string XEUSU_ID)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string queryEliminarUsuario = @"DELETE FROM XEUSU_USUAR 
                                                    WHERE XEUSU_ID = @XEUSU_ID";

                    SqlCommand cmdUsuario = new SqlCommand(queryEliminarUsuario, oConexion);
                    cmdUsuario.Parameters.AddWithValue("@XEUSU_ID", XEUSU_ID);

                    oConexion.Open();
                    int rowsAffected = cmdUsuario.ExecuteNonQuery();
                    respuesta = rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error en EliminarUsuario: " + ex.Message);
                }
            }
            return respuesta;
        }

        // Método para obtener usuarios con información detallada (incluye nombres)
        public List<Usuario> ObtenerUsuariosDetallados()
        {
            List<Usuario> rptListaUsuario = new List<Usuario>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                string query = @"SELECT U.XEUSU_ID, U.XEROL_ID, U.PEPER_ID, U.MECARR_ID, U.MEEST_ID, 
                                        U.XEUSU_NOMBRE, U.XEUSU_CONTRA, U.XEUSU_ESTADO,
                                        R.XEROL_NOMBRE,
                                        P.PEPER_NOMBRE + ' ' + P.PEPER_APELLIDO AS PersonaNombre,
                                        C.MECARR_NOMBRE
                                 FROM XEUSU_USUAR U
                                 LEFT JOIN XEROL_ROL R ON U.XEROL_ID = R.XEROL_ID
                                 LEFT JOIN PEPER_PERSON P ON U.PEPER_ID = P.PEPER_ID
                                 LEFT JOIN MECARR_CARRERA C ON U.MECARR_ID = C.MECARR_ID
                                 ORDER BY U.XEUSU_ID";

                SqlCommand cmd = new SqlCommand(query, oConexion);

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        rptListaUsuario.Add(new Usuario()
                        {
                            XEUSU_ID = dr["XEUSU_ID"]?.ToString(),
                            XEROL_ID = dr["XEROL_ID"]?.ToString(),
                            PEPER_ID = dr["PEPER_ID"]?.ToString(),
                            MECARR_ID = dr["MECARR_ID"]?.ToString(),
                            MEEST_ID = dr["MEEST_ID"]?.ToString(),
                            XEUSU_NOMBRE = dr["XEUSU_NOMBRE"]?.ToString(),
                            XEUSU_CONTRA = dr["XEUSU_CONTRA"]?.ToString(),
                            XEUSU_ESTADO = dr["XEUSU_ESTADO"]?.ToString(),
                            
                           
                        });
                    }
                    dr.Close();
                    return rptListaUsuario;
                }
                catch (Exception ex)
                {
                    rptListaUsuario = null;
                    System.Diagnostics.Debug.WriteLine("Error en ObtenerUsuariosDetallados: " + ex.Message);
                    return rptListaUsuario;
                }
            }
        }

        // Método para buscar usuarios
        public List<Usuario> BuscarUsuario(string criterio, string valor)
        {
            List<Usuario> rptListaUsuario = new List<Usuario>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string query = @"SELECT XEUSU_ID, XEROL_ID, PEPER_ID, MECARR_ID, MEEST_ID, 
                                            XEUSU_NOMBRE, XEUSU_CONTRA, XEUSU_ESTADO
                                     FROM XEUSU_USUAR 
                                     WHERE ";

                    switch (criterio.ToUpper())
                    {
                        case "NOMBRE":
                            query += "XEUSU_NOMBRE LIKE @VALOR";
                            break;
                        case "ESTADO":
                            query += "XEUSU_ESTADO = @VALOR";
                            break;
                        case "ROL":
                            query += "XEROL_ID = @VALOR";
                            break;
                        case "PERSONAL":
                            query += "PEPER_ID = @VALOR";
                            break;
                        case "ESTUDIANTE":
                            query += "MEEST_ID IS NOT NULL AND MECARR_ID = @VALOR";
                            break;
                        default:
                            query += "XEUSU_NOMBRE LIKE @VALOR";
                            break;
                    }

                    query += " ORDER BY XEUSU_ID";

                    SqlCommand cmd = new SqlCommand(query, oConexion);

                    if (criterio.ToUpper() == "NOMBRE")
                    {
                        cmd.Parameters.AddWithValue("@VALOR", "%" + valor + "%");
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@VALOR", valor);
                    }

                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        rptListaUsuario.Add(new Usuario()
                        {
                            XEUSU_ID = dr["XEUSU_ID"]?.ToString(),
                            XEROL_ID = dr["XEROL_ID"]?.ToString(),
                            PEPER_ID = dr["PEPER_ID"]?.ToString(),
                            MECARR_ID = dr["MECARR_ID"]?.ToString(),
                            MEEST_ID = dr["MEEST_ID"]?.ToString(),
                            XEUSU_NOMBRE = dr["XEUSU_NOMBRE"]?.ToString(),
                            XEUSU_CONTRA = dr["XEUSU_CONTRA"]?.ToString(),
                            XEUSU_ESTADO = dr["XEUSU_ESTADO"]?.ToString()
                        });
                    }
                    dr.Close();
                }
                catch (Exception ex)
                {
                    rptListaUsuario = null;
                    System.Diagnostics.Debug.WriteLine("Error en BuscarUsuario: " + ex.Message);
                }
            }
            return rptListaUsuario;
        }

        // Validar que el nombre de usuario sea único
        public bool ValidarUsuarioUnico(string XEUSU_NOMBRE, string XEUSU_ID_EXCLUIR = null)
        {
            bool esUnico = true;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string query;
                    SqlCommand cmd;

                    if (string.IsNullOrEmpty(XEUSU_ID_EXCLUIR))
                    {
                        query = @"SELECT COUNT(*) FROM XEUSU_USUAR 
                                   WHERE XEUSU_NOMBRE = @XEUSU_NOMBRE";
                        cmd = new SqlCommand(query, oConexion);
                        cmd.Parameters.AddWithValue("@XEUSU_NOMBRE", XEUSU_NOMBRE);
                    }
                    else
                    {
                        query = @"SELECT COUNT(*) FROM XEUSU_USUAR 
                                   WHERE XEUSU_NOMBRE = @XEUSU_NOMBRE 
                                   AND XEUSU_ID != @XEUSU_ID";
                        cmd = new SqlCommand(query, oConexion);
                        cmd.Parameters.AddWithValue("@XEUSU_NOMBRE", XEUSU_NOMBRE);
                        cmd.Parameters.AddWithValue("@XEUSU_ID", XEUSU_ID_EXCLUIR);
                    }

                    oConexion.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    esUnico = count == 0;
                }
                catch (Exception ex)
                {
                    esUnico = false;
                    System.Diagnostics.Debug.WriteLine("Error en ValidarUsuarioUnico: " + ex.Message);
                }
            }
            return esUnico;
        }

        // Método para cambiar contraseña
        public int CambiarClave(string XEUSU_NOMBRE, string XEUSU_CONTRA, string nuevaClave)
        {
            int res = 0;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    // Encriptar contraseña actual para verificar
                    string contrasenaActualEncriptada = Encriptar(XEUSU_CONTRA);

                    // Primero verificar que la contraseña actual es correcta
                    string queryVerificar = @"SELECT COUNT(*) FROM XEUSU_USUAR 
                                             WHERE XEUSU_NOMBRE = @XEUSU_NOMBRE 
                                             AND XEUSU_CONTRA = @XEUSU_CONTRA";

                    SqlCommand cmdVerificar = new SqlCommand(queryVerificar, oConexion);
                    cmdVerificar.Parameters.AddWithValue("@XEUSU_NOMBRE", XEUSU_NOMBRE);
                    cmdVerificar.Parameters.AddWithValue("@XEUSU_CONTRA", contrasenaActualEncriptada);

                    oConexion.Open();

                    int existe = Convert.ToInt32(cmdVerificar.ExecuteScalar());

                    if (existe > 0)
                    {
                        // Actualizar contraseña
                        string nuevaClaveEncriptada = Encriptar(nuevaClave);
                        string queryActualizar = @"UPDATE XEUSU_USUAR 
                                                  SET XEUSU_CONTRA = @NUEVA_CLAVE 
                                                  WHERE XEUSU_NOMBRE = @XEUSU_NOMBRE";

                        SqlCommand cmdActualizar = new SqlCommand(queryActualizar, oConexion);
                        cmdActualizar.Parameters.AddWithValue("@NUEVA_CLAVE", nuevaClaveEncriptada);
                        cmdActualizar.Parameters.AddWithValue("@XEUSU_NOMBRE", XEUSU_NOMBRE);

                        cmdActualizar.ExecuteNonQuery();
                        res = 1;
                    }
                    else
                    {
                        res = 0; // Contraseña actual incorrecta
                    }
                }
                catch (Exception ex)
                {
                    res = 0;
                    System.Diagnostics.Debug.WriteLine("Error en CambiarClave: " + ex.Message);
                }
            }
            return res;
        }

        // Métodos de encriptación
        public string Encriptar(string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;

            char remplaza;
            string re_incrementa = "";
            for (int i = 0; i < str.Length; i++)
            {
                remplaza = (char)((int)str[i] + 5);
                re_incrementa = re_incrementa + remplaza.ToString();
            }
            return re_incrementa;
        }

        public string DesEncriptar(string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;

            char remplaza;
            string re_incrementa = "";
            for (int i = 0; i < str.Length; i++)
            {
                remplaza = (char)((int)str[i] - 5);
                re_incrementa = re_incrementa + remplaza.ToString();
            }
            return re_incrementa;
        }

        // Método para obtener los roles disponibles
        public List<Rol> ObtenerRolesDisponibles()
        {
            List<Rol> listaRoles = new List<Rol>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                string query = @"SELECT XEROL_ID, XEROL_NOMBRE 
                                 FROM XEROL_ROL 
                                 ORDER BY XEROL_NOMBRE";

                SqlCommand cmd = new SqlCommand(query, oConexion);

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        listaRoles.Add(new Rol()
                        {
                            XEROL_ID = dr["XEROL_ID"]?.ToString(),
                            XEROL_NOMBRE = dr["XEROL_NOMBRE"]?.ToString()
                        });
                    }
                    dr.Close();
                }
                catch (Exception ex)
                {
                    listaRoles = null;
                    System.Diagnostics.Debug.WriteLine("Error en ObtenerRolesDisponibles: " + ex.Message);
                }
            }
            return listaRoles;
        }

        // Método para obtener personal disponible (sin usuario asignado)
        public List<Personal> ObtenerPersonalDisponible()
        {
            List<Personal> listaPersonal = new List<Personal>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                string query = @"SELECT P.PEPER_ID, P.PEPER_NOMBRE, P.PEPER_APELLIDO
                                 FROM PEPER_PERSON P
                                 LEFT JOIN XEUSU_USUAR U ON P.PEPER_ID = U.PEPER_ID
                                 WHERE U.PEPER_ID IS NULL
                                 ORDER BY P.PEPER_NOMBRE, P.PEPER_APELLIDO";

                SqlCommand cmd = new SqlCommand(query, oConexion);

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        listaPersonal.Add(new Personal()
                        {
                            PEPER_ID = dr["PEPER_ID"]?.ToString(),
                            PEPER_NOMBRE = dr["PEPER_NOMBRE"]?.ToString(),
                            PEPER_APELLIDO = dr["PEPER_APELLIDO"]?.ToString()
                        });
                    }
                    dr.Close();
                }
                catch (Exception ex)
                {
                    listaPersonal = null;
                    System.Diagnostics.Debug.WriteLine("Error en ObtenerPersonalDisponible: " + ex.Message);
                }
            }
            return listaPersonal;
        }
        public bool ActualizarRolUsuario(string usuarioId, string rolId)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string query = @"UPDATE XEUSU_USUAR 
                                 SET XEROL_ID = @XEROL_ID
                                 WHERE XEUSU_ID = @XEUSU_ID";

                    SqlCommand cmd = new SqlCommand(query, oConexion);

                    // Solo actualiza el rol
                    cmd.Parameters.AddWithValue("@XEUSU_ID", usuarioId);

                    if (string.IsNullOrEmpty(rolId))
                        cmd.Parameters.AddWithValue("@XEROL_ID", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@XEROL_ID", rolId);

                    oConexion.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    respuesta = rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error en ActualizarRolUsuario: " + ex.Message);
                }
            }
            return respuesta;
        }
    }
}