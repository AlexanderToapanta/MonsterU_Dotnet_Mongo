using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CapaModelo;

namespace CapaDatos
{
    public class CD_Carrera
    {
        private static CD_Carrera _instancia = null;
        private CD_Carrera() { }

        public static CD_Carrera Instancia
        {
            get
            {
                if (_instancia == null) _instancia = new CD_Carrera();
                return _instancia;
            }
        }

        public List<Carrera> ObtenerCarreras()
        {
            var lista = new List<Carrera>();
            using (SqlConnection cn = new SqlConnection(Conexion.CN))
            {
                string query = @"SELECT MECARR_ID, MECARR_NOMBRE, MECARR_MAX_CRED, MECARR_MIN_CRED
 FROM MECARR_CARRERA
 ORDER BY MECARR_ID";
                SqlCommand cmd = new SqlCommand(query, cn);
                try
                {
                    cn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Carrera
                            {
                                MECARR_ID = dr["MECARR_ID"]?.ToString(),
                                MECARR_NOMBRE = dr["MECARR_NOMBRE"]?.ToString(),
                                MECARR_MAXCRED = dr["MECARR_MAX_CRED"] != DBNull.Value ? Convert.ToInt32(dr["MECARR_MAX_CRED"]) : 0,
                                MECARR_MINCRED = dr["MECARR_MIN_CRED"] != DBNull.Value ? Convert.ToInt32(dr["MECARR_MIN_CRED"]) : 0
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    lista = null;
                }
            }
            return lista;
        }

        public Carrera ObtenerCarreraPorId(string id)
        {
            Carrera carrera = null;
            using (SqlConnection cn = new SqlConnection(Conexion.CN))
            {
                string query = @"SELECT MECARR_ID, MECARR_NOMBRE, MECARR_MAX_CRED, MECARR_MIN_CRED
 FROM MECARR_CARRERA
 WHERE MECARR_ID = @MECARR_ID";
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@MECARR_ID", id);
                try
                {
                    cn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            carrera = new Carrera
                            {
                                MECARR_ID = dr["MECARR_ID"]?.ToString(),
                                MECARR_NOMBRE = dr["MECARR_NOMBRE"]?.ToString(),
                                MECARR_MAXCRED = dr["MECARR_MAX_CRED"] != DBNull.Value ? Convert.ToInt32(dr["MECARR_MAX_CRED"]) : 0,
                                MECARR_MINCRED = dr["MECARR_MIN_CRED"] != DBNull.Value ? Convert.ToInt32(dr["MECARR_MIN_CRED"]) : 0
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    carrera = null;
                }
            }
            return carrera;
        }

        public bool RegistrarCarrera(Carrera c)
        {
            bool resultado = false;
            using (SqlConnection cn = new SqlConnection(Conexion.CN))
            {
                string query = @"INSERT INTO MECARR_CARRERA (MECARR_ID, MECARR_NOMBRE, MECARR_MAX_CRED, MECARR_MIN_CRED)
 VALUES (@MECARR_ID, @MECARR_NOMBRE, @MECARR_MAX_CRED, @MECARR_MIN_CRED)";
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@MECARR_ID", c.MECARR_ID);
                cmd.Parameters.AddWithValue("@MECARR_NOMBRE", c.MECARR_NOMBRE ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@MECARR_MAX_CRED", c.MECARR_MAXCRED);
                cmd.Parameters.AddWithValue("@MECARR_MIN_CRED", c.MECARR_MINCRED);
                try
                {
                    cn.Open();
                    int rows = cmd.ExecuteNonQuery();
                    resultado = rows > 0;
                }
                catch (Exception ex)
                {
                    resultado = false;
                }
            }
            return resultado;
        }

        public List<Carrera> BuscarCarreras(string nombre, int? creditosMin, int? creditosMax)
        {
            var lista = new List<Carrera>();
            using (SqlConnection cn = new SqlConnection(Conexion.CN))
            {
                string query = @"SELECT MECARR_ID, MECARR_NOMBRE, MECARR_MAX_CRED, MECARR_MIN_CRED
                         FROM MECARR_CARRERA
                         WHERE (@Nombre IS NULL OR MECARR_NOMBRE LIKE '%' + @Nombre + '%')
                           AND (@MinCred IS NULL OR MECARR_MIN_CRED >= @MinCred)
                           AND (@MaxCred IS NULL OR MECARR_MAX_CRED <= @MaxCred)
                         ORDER BY MECARR_NOMBRE";

                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@Nombre", string.IsNullOrWhiteSpace(nombre) ? (object)DBNull.Value : nombre);
                cmd.Parameters.AddWithValue("@MinCred", creditosMin.HasValue ? (object)creditosMin.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@MaxCred", creditosMax.HasValue ? (object)creditosMax.Value : DBNull.Value);

                try
                {
                    cn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Carrera
                            {
                                MECARR_ID = dr["MECARR_ID"]?.ToString(),
                                MECARR_NOMBRE = dr["MECARR_NOMBRE"]?.ToString(),
                                MECARR_MAXCRED = dr["MECARR_MAX_CRED"] != DBNull.Value ? Convert.ToInt32(dr["MECARR_MAX_CRED"]) : 0,
                                MECARR_MINCRED = dr["MECARR_MIN_CRED"] != DBNull.Value ? Convert.ToInt32(dr["MECARR_MIN_CRED"]) : 0
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    lista = null;
                }
            }
            return lista;
        }

        public bool ModificarCarrera(Carrera c)
        {
            bool resultado = false;
            using (SqlConnection cn = new SqlConnection(Conexion.CN))
            {
                string query = @"UPDATE MECARR_CARRERA SET
 MECARR_NOMBRE = @MECARR_NOMBRE,
 MECARR_MAX_CRED = @MECARR_MAX_CRED,
 MECARR_MIN_CRED = @MECARR_MIN_CRED
 WHERE MECARR_ID = @MECARR_ID";
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@MECARR_ID", c.MECARR_ID);
                cmd.Parameters.AddWithValue("@MECARR_NOMBRE", c.MECARR_NOMBRE ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@MECARR_MAX_CRED", c.MECARR_MAXCRED);
                cmd.Parameters.AddWithValue("@MECARR_MIN_CRED", c.MECARR_MINCRED);
                try
                {
                    cn.Open();
                    int rows = cmd.ExecuteNonQuery();
                    resultado = rows > 0;
                }
                catch (Exception ex)
                {
                    resultado = false;
                }
            }
            return resultado;
        }

        public bool EliminarCarrera(string id)
        {
            bool resultado = false;
            using (SqlConnection cn = new SqlConnection(Conexion.CN))
            {
                string query = @"DELETE FROM MECARR_CARRERA WHERE MECARR_ID = @MECARR_ID";
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@MECARR_ID", id);
                try
                {
                    cn.Open();
                    int rows = cmd.ExecuteNonQuery();
                    resultado = rows > 0;
                }
                catch (Exception ex)
                {
                    resultado = false;
                }
            }
            return resultado;
        }

        public bool EliminarMultiple(List<string> ids)
        {
            if (ids == null || ids.Count == 0) return false;
            bool resultado = false;
            using (SqlConnection cn = new SqlConnection(Conexion.CN))
            {
                cn.Open();
                using (SqlTransaction tr = cn.BeginTransaction())
                {
                    try
                    {
                        foreach (var id in ids)
                        {
                            string query = @"DELETE FROM MECARR_CARRERA WHERE MECARR_ID = @MECARR_ID";
                            SqlCommand cmd = new SqlCommand(query, cn, tr);
                            cmd.Parameters.AddWithValue("@MECARR_ID", id);
                            cmd.ExecuteNonQuery();
                        }
                        tr.Commit();
                        resultado = true;
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        resultado = false;
                    }
                }
            }
            return resultado;
        }

        public string GenerarNuevoId()
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(Conexion.CN))
                {
                    string query = "SELECT MAX(MECARR_ID) FROM MECARR_CARRERA";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    cn.Open();
                    object result = cmd.ExecuteScalar();
                    string maxId = result == null || result == DBNull.Value ? null : result.ToString();

                    if (string.IsNullOrEmpty(maxId))
                        return "00001";

                    // extract digits from maxId
                    string digits = new string(maxId.Where(char.IsDigit).ToArray());
                    if (!string.IsNullOrEmpty(digits) && int.TryParse(digits, out int n))
                    {
                        n++;
                        return n.ToString().PadLeft(5, '0');
                    }

                    // If cannot parse, fallback to numeric increment of full value if possible
                    if (int.TryParse(maxId, out int m))
                    {
                        m++;
                        return m.ToString().PadLeft(5, '0');
                    }

                    // last resort
                    return "00001";
                }
            }
            catch
            {
                return "00001";
            }
        }
    }
}
