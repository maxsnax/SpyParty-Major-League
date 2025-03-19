<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Scoreboard.aspx.cs" Inherits="SML.Scoreboard" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    
    <style>
        body {
            background-color: #D9D9BF;
        }
    </style>

    <main>
        
        <section aria-labelledby="aspnetTitle">
            <h1 id="title" style="text-align: center; margin: 15px">Current Rankings</h1>

            
            <asp:Panel runat="server" ID="masterTablePanel" CssClass="master-table">
                <asp:DropDownList ID="selectSeasonList" runat="server" CssClass="seasonDropDown"></asp:DropDownList>
            </asp:Panel>
            

        </section>

        

        
    </main>

</asp:Content>
