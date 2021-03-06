﻿//Need to add comments to the methods
//Need to take out the SQL from the for notation.
//Need to use try/catch notation only in the places needed. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

//This is the logic of the employee page
public partial class Workers_Employee : System.Web.UI.Page
{
    bool isLogged = System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
    private ShiftTable weeklyShiftTable = new ShiftTable();

    //On page load we fill the weekly shift table on this page, if the user is diconnected we redirect him to the main page
    protected void Page_Load(object sender, EventArgs e)
    {
        if (isLogged)
        {
            string orgName = System.Web.HttpContext.Current.User.Identity.Name.Split(' ')[0].Trim();

            fillWeeklyShiftTable(orgName);
            
            fillTable(orgName);
        }
        else
        {
            Response.Redirect("~/Account/Login.aspx");
        }
    }

    //this method fills the list that includes the information about the weekly shift table
    private void fillWeeklyShiftTable(string org_name)
    {
        SqlConnection conn = new SqlConnection(getConnectionString());
        string sql = "SELECT [Day], [Begin Time], [End Time], [Shift Info], [Worker ID] FROM [Shift Schedule] WHERE [Organization Name] = '" + org_name + "'";
        try
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand(sql, conn);

            SqlDataReader myReader = cmd.ExecuteReader();
            if (!myReader.HasRows)
            {
                return;
            }
            while (myReader.Read())
            {
                Shift shiftInWeek = new Shift(null, null, null, null, null, null, null, null);
                shiftInWeek.setDay(myReader[0].ToString().Trim());
                shiftInWeek.setBegin_Time(myReader[1].ToString().Trim());
                shiftInWeek.setEnd_Time(myReader[2].ToString().Trim());
                shiftInWeek.setWorker_ID(myReader[4].ToString().Trim());
                shiftInWeek.setOrganization(org_name);
                weeklyShiftTable.AddShift(shiftInWeek);
            }
            myReader.Close();
            if (weeklyShiftTable.getShiftFromTable(0).getWroker_ID() != null && weeklyShiftTable.getShiftFromTable(0).getWroker_ID() != "NULL")
            {
                sql = "SELECT [ID], [First Name], [Last Name] FROM [Worker] WHERE [Organization Name] = '" + org_name + "'";
                cmd = new SqlCommand(sql, conn);
                myReader = cmd.ExecuteReader();
                while (myReader.Read())
                {
                    for (int i = 0; i < weeklyShiftTable.tableSize(); i++)
                    {
                        if (weeklyShiftTable.getShiftFromTable(i).getWroker_ID().Equals(myReader[0].ToString().Trim()))
                        {
                            weeklyShiftTable.getShiftFromTable(i).setName(myReader[1].ToString().Trim() + " " + myReader[2].ToString().Trim());
                        }
                    }
                }
            }
        }
        catch (System.Data.SqlClient.SqlException ex)
        {
            string msg = "Insert Error:";
            msg += ex.Message;
            throw new Exception(msg);
        }
        finally
        {
            conn.Close();
        }
    }

    //returns the sql connection string
    private string getConnectionString()
    {
        //sets the connection string from your web config file "ConnString" is the name of your Connection String
        return System.Configuration.ConfigurationManager.ConnectionStrings["ShifterManDB"].ConnectionString;
    }

    //creates the grid view table of the weekly shift table on the employee.aspx page
    private void fillTable(string org_name)
    {
        DataTable dt = new DataTable();

        DataColumn dcHourDay = new DataColumn("HourDay", typeof(string));
        DataColumn dcSunday = new DataColumn("Sunday", typeof(string));
        DataColumn dcMonday = new DataColumn("Monday", typeof(string));
        DataColumn dcTusday = new DataColumn("Tusday", typeof(string));
        DataColumn dcWednsday = new DataColumn("Wednsday", typeof(string));
        DataColumn dcThursday = new DataColumn("Thursday", typeof(string));
        DataColumn dcFriday = new DataColumn("Friday", typeof(string));
        DataColumn dcSaturday = new DataColumn("Saturday", typeof(string));

        dt.Columns.AddRange(new DataColumn[] { dcHourDay, dcSunday, dcMonday, dcTusday, dcWednsday, dcThursday, dcFriday, dcSaturday });

        SqlConnection conn = new SqlConnection(getConnectionString());
        string sql = "SELECT DISTINCT [Begin Time], [End Time], [Shift Info] FROM [Shift Schedule] WHERE [Organization Name] = '" + org_name + "'";
        try
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand(sql, conn);

            SqlDataReader myReader = cmd.ExecuteReader();
            if (!myReader.HasRows)
            {
                return;
            }
            while (myReader.Read())
            {
                int numOfWorkers = Convert.ToInt16(myReader["Shift Info"].ToString().Trim());
                for (int i = 0; i < numOfWorkers; i++)
                {
                    dt.Rows.Add(new object[] { myReader["Begin Time"].ToString().Trim() + "-" + myReader["End Time"].ToString().Trim()/* + " -> " + myReader["Shift Info"].ToString().Trim() + " Workers In Shift"*/, "", "", "", "", "", "", "" });
                }
//                dt.Rows.Add(new object[] { "----------------------", "----------------------", "----------------------", "----------------------", "----------------------", "----------------------", "----------------------", "----------------------" });
            }
            WeeklyScheduleGrid.DataSource = dt;
            WeeklyScheduleGrid.DataBind();
        }
        catch (System.Data.SqlClient.SqlException ex)
        {
            string msg = "Insert Error:";
            msg += ex.Message;
            throw new Exception(msg);
        }
        finally
        {
            conn.Close();
        }
        fillWeeklySchedule();
    }

    //fills the shifts on the grid view table of the weekly shift table on the employee.aspx page acoording the list that we filled in earlier
    private void fillWeeklySchedule()
    {
        int index = 0;
        if (weeklyShiftTable.getShiftFromTable(0).getName() != null)
        {
            foreach (Shift sh in weeklyShiftTable.GetAllShifts())
            {
                switch (sh.getDay())
                {
                    case "Sunday":
                        index = 1;
                        break;
                    case "Monday":
                        index = 2;
                        break;
                    case "Tusday":
                        index = 3;
                        break;
                    case "Wednsday":
                        index = 4;
                        break;
                    case "Thursday":
                        index = 5;
                        break;
                    case "Friday":
                        index = 6;
                        break;
                    case "Saturday":
                        index = 7;
                        break;
                }
                for (int i = 0; i < WeeklyScheduleGrid.Rows.Count; i++)
                {
                    if (WeeklyScheduleGrid.Rows[i].Cells[0].Text.Trim().Split('-')[0].Trim().Equals(sh.getBegin_Time()) &&
                        WeeklyScheduleGrid.Rows[i].Cells[0].Text.Trim().Split('-')[1].Trim().Equals(sh.getEnd_Time()) &&
                        WeeklyScheduleGrid.Rows[i].Cells[index].Text.Trim() == "&nbsp;")
                    {
                        WeeklyScheduleGrid.Rows[i].Cells[index].Text = sh.getName();
                        break;
                    }
                }
            }
        }
    }
}
