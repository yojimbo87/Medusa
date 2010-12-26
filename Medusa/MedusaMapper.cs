using System;
using System.Collections.Generic;
using System.Reflection;
using System.Data;

using MySql.Data.MySqlClient;

namespace Medusa
{
    public class MedusaMapper
    {
        public string ConnectionString { get; set; }
        public MySqlConnection Connection { get; set; }
        public MySqlCommand Command { get; set; }
        public List<DbParameter> OutParameters { get; private set; }

        public MedusaMapper() { }

        public MedusaMapper(string connectionString)
        {
            ConnectionString = connectionString;
        }

        private void Open()
        {
            try
            {
                Connection = new MySqlConnection(ConnectionString);
                Connection.Open();

            }
            catch (Exception ex)
            {
                Connection.Close();
                throw new Exception(ex.Message);
            }
        }

        private void Close()
        {
            if (Connection != null)
            {
                Connection.Close();
            }
        }

        private object ExecuteProcedure(string procedureName, ExecuteType executeType, List<DbParameter> parameters)
        {
            object returnObject = null;

            if (Connection != null)
            {
                if (Connection.State == ConnectionState.Open)
                {
                    Command = new MySqlCommand(procedureName, Connection);
                    Command.CommandType = CommandType.StoredProcedure;

                    if (parameters != null)
                    {
                        Command.Parameters.Clear();

                        foreach (DbParameter dbParameter in parameters)
                        {
                            MySqlParameter parameter = new MySqlParameter();
                            parameter.ParameterName = "@" + dbParameter.Name;
                            parameter.Direction = (ParameterDirection)dbParameter.Direction;
                            parameter.Value = dbParameter.Value;
                            Command.Parameters.Add(parameter);
                        }
                    }

                    switch (executeType)
                    {
                        case ExecuteType.ExecuteReader:
                            returnObject = Command.ExecuteReader();
                            break;
                        case ExecuteType.ExecuteNonQuery:
                            returnObject = Command.ExecuteNonQuery();
                            break;
                        case ExecuteType.ExecuteScalar:
                            returnObject = Command.ExecuteScalar();
                            break;
                        default:
                            break;
                    }
                }
            }

            return returnObject;
        }

        private void UpdateOutParameters()
        {
            if (Command.Parameters.Count > 0)
            {
                OutParameters = new List<DbParameter>();
                OutParameters.Clear();

                for (int i = 0; i < Command.Parameters.Count; i++)
                {
                    if (Command.Parameters[i].Direction == ParameterDirection.Output)
                    {
                        OutParameters.Add(new DbParameter(Command.Parameters[i].ParameterName, DbDirection.Output, Command.Parameters[i].Value));
                    }
                }
            }
        }

        public T ExecuteSingle<T>(string procedureName) where T : new()
        {
            return ExecuteSingle<T>(procedureName, null);
        }

        public T ExecuteSingle<T>(string procedureName, List<DbParameter> parameters) where T : new()
        {
            Open();
            IDataReader reader = (IDataReader)ExecuteProcedure(procedureName, ExecuteType.ExecuteReader, parameters);
            T tempObject = new T();

            if (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    PropertyInfo propertyInfo = typeof(T).GetProperty(reader.GetName(i));
                    propertyInfo.SetValue(tempObject, reader.GetValue(i), null);
                }
            }

            reader.Close();

            UpdateOutParameters();

            Close();

            return tempObject;
        }

        public List<T> ExecuteList<T>(string procedureName) where T : new()
        {
            return ExecuteList<T>(procedureName, null);
        }

        public List<T> ExecuteList<T>(string procedureName, List<DbParameter> parameters) where T : new()
        {
            List<T> objects = new List<T>();

            Open();
            IDataReader reader = (IDataReader)ExecuteProcedure(procedureName, ExecuteType.ExecuteReader, parameters);

            while (reader.Read())
            {
                T tempObject = new T();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.GetValue(i) != DBNull.Value)
                    {
                        PropertyInfo propertyInfo = typeof(T).GetProperty(reader.GetName(i));
                        propertyInfo.SetValue(tempObject, reader.GetValue(i), null);
                    }
                }

                objects.Add(tempObject);
            }

            reader.Close();

            UpdateOutParameters();

            Close();

            return objects;
        }

        public int ExecuteNonQuery(string procedureName, List<DbParameter> parameters)
        {
            int returnValue;

            Open();

            returnValue = (int)ExecuteProcedure(procedureName, ExecuteType.ExecuteNonQuery, parameters);

            UpdateOutParameters();

            Close();

            return returnValue;
        }
    }

    public enum ExecuteType
    {
        ExecuteReader,
        ExecuteNonQuery,
        ExecuteScalar
    };

    public enum DbDirection
    {
        Input,
        InputOutput,
        Output,
        ReturnValue
    }

    public class DbParameter
    {
        public string Name { get; set; }
        public DbDirection Direction { get; set; }
        public object Value { get; set; }

        public DbParameter() { }

        public DbParameter(string parameterName, DbDirection parameterDirection, object parameterValue)
        {
            Name = parameterName;
            Direction = parameterDirection;
            Value = parameterValue;
        }
    }
}
