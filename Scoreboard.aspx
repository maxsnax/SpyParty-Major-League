<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Scoreboard.aspx.cs" Inherits="SML.Scoreboard" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    
    <style>
        body {
            background-color: #D9D9BF;
        }
    </style>

    <main>
        
        <section class="row" aria-labelledby="aspnetTitle">
            <h1 id="title" style="text-align: center">Current Scoreboard</h1>

            <asp:DropDownList ID="seasonSelectList" runat="server" style="padding: 10px; margin: 10px;"></asp:DropDownList>

            <div id="tableHolder" runat="server">
                <asp:Panel runat="server" ID="masterTablePanel"></asp:Panel>
            </div>

        </section>

        

        
    </main>

</asp:Content>
