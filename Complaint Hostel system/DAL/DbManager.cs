using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Web;
using Complaint_Hostel_system.Models;

namespace Complaint_Hostel_system.DAL
{
    public class DbManager : IDisposable
    {
        private readonly string connectString =
            ConfigurationManager.ConnectionStrings["complaintHostelDB"].ConnectionString;

        private SqlConnection _connection;

        public DbManager()
        {
            _connection = new SqlConnection(connectString);
            _connection.Open();
        }

        public SqlConnection Connection => _connection;
        public void Dispose()
        {
            if (_connection != null)
            {
                _connection = null;
            }
        }

        #region REGISTER NEW USER LOGIC :
        public int RegisterUser(string name, string email, string password, string roomNo)
        {
            using (var con = new SqlConnection(connectString))
            using (var cmd = new SqlCommand(
                @"INSERT INTO Users (Name, Email, PasswordHash, RoomNo)
                  VALUES (@Name, @Email, @Password, @RoomNo)", con))
            {
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", password); // we'll hash later
                cmd.Parameters.AddWithValue("@RoomNo", roomNo);

                con.Open();
                return cmd.ExecuteNonQuery(); // rows affected
            }
        }

        // Check if email already exists
        public bool IsEmailTaken(string email)
        {
            using (var con = new SqlConnection(connectString))
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Users WHERE Email=@Email", con))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                con.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }
        #endregion


        #region Validate login (returns userId if ok, 0 if invalid)
        public User ValidateLogin(string email, string password)
        {
            using (var con = new SqlConnection(connectString))
            using (var cmd = new SqlCommand(
                @"SELECT UserId , Name, Email, RoomNo FROM Users
                  WHERE Email=@Email AND PasswordHash=@Password", con))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", password);
                con.Open();

                using (var reader = cmd.ExecuteReader())
                { if (reader.Read())
                    {
                        return new User
                        {
                            UserId = Convert.ToInt32(reader["UserId"]),
                            Name = reader["Name"].ToString(),
                            Email = reader["Email"].ToString(),
                            RoomNo = reader["RoomNo"].ToString(),
                        };
                    }
                }
            }
            return null;
        }
        #endregion

        #region CREATE COMPLAINT LOGIC:
        public bool AddComplaint(Complaint complaint)
        {
            using (SqlConnection con = new SqlConnection(connectString))
            {
                string query = "INSERT INTO Complaints (UserId, Title, Description, CreatedAt, Status) " +
                               "VALUES (@UserId, @Title, @Description, @CreatedAt, @Status)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserId", complaint.UserId);
                cmd.Parameters.AddWithValue("@Title", complaint.Title);
                cmd.Parameters.AddWithValue("@Description", complaint.Description);
                cmd.Parameters.AddWithValue("@CreatedAt", complaint.CreatedAt);
                cmd.Parameters.AddWithValue("@Status", complaint.Status);

                con.Open();
                int rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
        }
       
        public List<Complaint> GetComplaintsByUser(int userId)
        {
            List<Complaint> list = new List<Complaint>();

            using (SqlConnection con = new SqlConnection(connectString))
            {
                string query = "SELECT * FROM Complaints WHERE UserId=@UserId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserId", userId);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Complaint c = new Complaint()
                    {
                        ComplaintId = (int)reader["ComplaintId"],
                        Title = reader["Title"].ToString(),
                        Description = reader["Description"].ToString(),
                        CreatedAt = (DateTime)reader["CreatedAt"],
                        Status = reader["Status"].ToString()
                    };
                    list.Add(c);
                }
            }

            return list;
        }
        #endregion


        #region ADMIN MODULE LOGIC :
        public AdminModel ValidateAdminLogin(string AdminEmail, string Admin_PasswordHash)
        {
            using (var con = new SqlConnection(connectString))
            using (var cmd = new SqlCommand(
                @"SELECT AdminId, AdminName, AdminEmail, Admin_PasswordHash
          FROM Admins
          WHERE AdminEmail=@AdminEmail AND Admin_PasswordHash=@Admin_PasswordHash", con))
            {
                cmd.Parameters.AddWithValue("@AdminEmail", AdminEmail);
                cmd.Parameters.AddWithValue("@Admin_PasswordHash", Admin_PasswordHash);

                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new AdminModel
                        {
                            AdminId = Convert.ToInt32(reader["AdminId"]),
                            AdminName = reader["AdminName"].ToString(),
                           AdminEmail = reader["AdminEmail"].ToString(),
                            Admin_PasswordHash = reader["Admin_PasswordHash"].ToString()
                        };
                    }
                }
            }
            return null;
        }


        public AdminModel GetAdminById(int adminId)
        {
            using (var con = new SqlConnection(connectString))
            using (var cmd = new SqlCommand(
                @"SELECT AdminId, AdminName, AdminEmail, Admin_PasswordHash
          FROM Admins
          WHERE AdminId = @AdminId", con))
            {
                cmd.Parameters.AddWithValue("@AdminId", adminId);
                con.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new AdminModel
                        {
                            AdminId = Convert.ToInt32(reader["AdminId"]),
                            AdminName = reader["AdminName"].ToString(),
                            AdminEmail = reader["AdminEmail"].ToString(),
                            Admin_PasswordHash = reader["Admin_PasswordHash"].ToString()
                        };
                    }
                }
            }
            return null;
        }

        #endregion

        #region  METHOD FOR DISPLAYING ALL THE COMPLAINTS FOR THE ADMIN:

        public List<Complaint> GetAllComplaints()
        {
            List<Complaint> complaints = new List<Complaint>();

            using (SqlConnection con = new SqlConnection(connectString))
            {
                string query = "SELECT ComplaintId, Title, Description, Status, CreatedAt, UserId FROM Complaints";
                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    Complaint complaint = new Complaint
                    {
                        ComplaintId = Convert.ToInt32(dr["ComplaintId"]),
                        Title = dr["Title"].ToString(),
                        Description = dr["Description"].ToString(),
                        Status = dr["Status"].ToString(),
                        CreatedAt = Convert.ToDateTime(dr["CreatedAt"]),
                        UserId = Convert.ToInt32(dr["UserId"])
                    };

                    complaints.Add(complaint);
                }
            }

            return complaints;
        }
        #endregion

        #region UPDATE COMPLAINT STATUS
 
        public void UpdateComplaintStatus(int complaintId, string status)
        {
            using (SqlConnection con = new SqlConnection(connectString))
            {
                string query = "UPDATE Complaints SET Status = @Status WHERE ComplaintId = @ComplaintId";
                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@ComplaintId", complaintId);

                //System.Diagnostics.Debug.WriteLine($"DEBUG: Executing query: {query}");
                //System.Diagnostics.Debug.WriteLine($"DEBUG: ComplaintId = {complaintId}, Status = '{status}'");

                con.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                //System.Diagnostics.Debug.WriteLine($"DEBUG: Rows affected: {rowsAffected}");

                //if (rowsAffected == 0)
                //{
                //    System.Diagnostics.Debug.WriteLine($"DEBUG: WARNING - No rows were updated! Check if ComplaintId {complaintId} exists.");
                //}
            }
        }
        #endregion

        #region delete complaint method:
        public void DeleteComplaint(int complaintId, int userId)
        {
            using (SqlConnection con = new SqlConnection(connectString))
            {
                string query = "DELETE FROM Complaints WHERE ComplaintId = @ComplaintId AND UserId = @UserId";
                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@ComplaintId", complaintId);
                cmd.Parameters.AddWithValue("@UserId", userId);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region Edit complaints by users:

        public Complaint GetComplaintForEdit(int complaintId)
        {
            Complaint complaint = null;

            using (SqlConnection con = new SqlConnection(connectString))
            {
                string query = "SELECT ComplaintId, Title, Description FROM Complaints WHERE ComplaintId=@ComplaintId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ComplaintId", complaintId);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    complaint = new Complaint()
                    {
                        ComplaintId = (int)reader["ComplaintId"],
                        Title = reader["Title"].ToString(),
                        Description = reader["Description"].ToString()
                    };
                }
            }

            return complaint;
        }

        public bool EditComplaintDetails(int complaintId, string title, string description)
        {
            using (SqlConnection con = new SqlConnection(connectString))
            {
                string query = "UPDATE Complaints SET Title=@Title, Description=@Description WHERE ComplaintId=@ComplaintId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Description", description);
                cmd.Parameters.AddWithValue("@ComplaintId", complaintId);

                con.Open();
                int rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
        }
        #endregion

        #region profile page for the users:

        // Get user details by ID
        public User GetUserById(int userId)
        {
            User user = null;
            using (SqlConnection con = new SqlConnection(connectString))
            {
                string query = "SELECT UserId, Name, Email, RoomNo FROM Users WHERE UserId = @UserId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserId", userId);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    user = new User
                    {
                        UserId = (int)reader["UserId"],
                        Name = reader["Name"].ToString(),
                        Email = reader["Email"].ToString(),
                        RoomNo = reader["RoomNo"].ToString()
                    };
                }
            }
            return user;
        }

        // Update user profile
        public void UpdateUserProfile(User user)
        {
            using (SqlConnection con = new SqlConnection(connectString))
            {
                string query = "UPDATE Users SET Name=@Name, Email=@Email, RoomNo=@RoomNo WHERE UserId=@UserId";
                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@Name", user.Name);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@RoomNo", user.RoomNo);
                cmd.Parameters.AddWithValue("@UserId", user.UserId);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region Forgot & Reset Password:

        // Save token + expiry after user requests reset
        public void SaveResetToken(string email, string token, DateTime expiry)
        {
            using (var con = new SqlConnection(connectString))
            using (var cmd = new SqlCommand( @"UPDATE Users SET ResetToken = @token, ResetTokenExpiry = @expiry
              WHERE Email = @Email", con))
            {
                cmd.Parameters.AddWithValue("@token", token);
                cmd.Parameters.AddWithValue("@expiry", expiry);
                cmd.Parameters.AddWithValue("@Email", email);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Verify token and update password
        public  bool ResetPassword(string token, string newPasswordHash)
        {
            using (var con = new SqlConnection(connectString))
            using (var cmd = new SqlCommand(
                @"UPDATE Users SET PasswordHash = @pwd, ResetToken = NULL, ResetTokenExpiry = NULL
                  WHERE ResetToken = @token AND ResetTokenExpiry > GETUTCDATE()", con))
            {
                cmd.Parameters.AddWithValue("@pwd", newPasswordHash);
                cmd.Parameters.AddWithValue("@token", token);
                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public DateTime? GetResetTokenExpiry(string email, string token)
        {
            using (var con = new SqlConnection(connectString))
            using (var cmd = new SqlCommand(
                "SELECT ResetTokenExpiry FROM Users " +
                "WHERE Email=@Email AND ResetToken=@Token", con))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Token", token);

                con.Open();
                var result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                    return null;

                return (DateTime)result;
            }
        }

        public void UpdatePassword(string email, string newPassword)
        {
            using (var con = new SqlConnection(connectString))
            using (var cmd = new SqlCommand(
               " UPDATE Users SET PasswordHash = @Password WHERE Email = @Email", con))
            {
                cmd.Parameters.AddWithValue("@Password", newPassword); // hash in production!
                cmd.Parameters.AddWithValue("@Email", email);

                con.Open();
              int rows = cmd.ExecuteNonQuery();
               HttpContext.Current.Items["Rows updated: "]= rows;
            }
        }

        #endregion

        #region Get  user be email:

        public User GetUserByEmail(string email)
        {
            using (var cmd = new SqlCommand("SELECT * FROM Users WHERE Email = @Email", _connection))
            {
                cmd.Parameters.AddWithValue("@Email", email);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            UserId = (int)reader["UserId"],
                            Email = (string)reader["Email"],
                            // map other fields…
                        };
                    }
                }
            }
            return null;
        }

        #endregion
    }

}
