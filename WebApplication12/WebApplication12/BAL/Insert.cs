using Microsoft.EntityFrameworkCore;
using WebApplication12.Models;
using WebApplication12;
using System;
using ClosedXML.Excel;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace WebApplication12.BAL
{
    public class Insert
    {
        private readonly MyDbContext _context;
        public Insert(MyDbContext context)
        {
            _context = context;
        }
       public async Task<List<Categorie>> ParseExcelFile1(Stream stream) 
        {
            
            var users= new List<Categorie>();
            using (var ws1=new XLWorkbook(stream))
            {
                var worksheet = ws1.Worksheet(1);
                var rows= worksheet.RangeUsed().RowsUsed().Skip(2);
                foreach (var row in rows)
                {
                    string Categories = row.Cell(2).GetValue<string>();

                    string result1 = Regex.Replace(Categories, @"[^a-zA-Z0-9. ]+","-");
                    bool IsUnique=!_context.Categories.Any(u=>u.Name== result1);
                    if(IsUnique)
                    {
                        var user = new Categorie
                        {
                            Name = result1
                        };
                        if (user.Name == "")
                        {
                            return users;
                        }
                        await _context.Categories.AddAsync(user);
                        await _context.SaveChangesAsync();
                        users.Add(user);                        
                    }  
                    
                }
            
            }
          
            return users;
        }
        public async Task<List<Skill>> ParseExcelFile2(Stream stream)
        {
            var users = new List<Skill>();
            int Cid1 = 0;
            using (var ws1 = new XLWorkbook(stream))
            {
                var worksheet = ws1.Worksheet(1);
                var rows = worksheet.RangeUsed().RowsUsed().Skip(2);
                foreach (var row in rows)
                {
                    string Skills = row.Cell(3).GetValue<string>();
                    string Categories = row.Cell(2).GetValue<string>();

                    string result2 = Regex.Replace(Skills, @"[^a-zA-Z0-9. ]+", "-");
                    string connectionString = "Data Source=PICSLIN024;Initial Catalog=new;Integrated Security=True;Trust Server Certificate=True";
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string sqlQuery = "SELECT Id FROM [dbo].[Categories] where Name=@Categories";
                        using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                        {
                            command.Parameters.AddWithValue("@Categories", Categories);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                     Cid1 = reader.GetInt32(0); 
                                                                        
                                }
                                
                            }
                        }



                    }
                    bool IsUnique = !_context.Skills.Any(u => u.Name == result2);

                    if (IsUnique)
                    {
                        var user = new Skill
                        {
                            Name = result2,
                            Cid = Cid1
                        };
                        if (user.Name == "")
                        {
                            return users;
                        }
                        await _context.Skills.AddAsync(user);
                        await _context.SaveChangesAsync();
                        users.Add(user);
                    }
                }

            }

            return users;
        }
    }
}
