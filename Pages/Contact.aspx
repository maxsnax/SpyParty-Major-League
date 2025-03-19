<%@ Page Title="Contact" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Contact.aspx.cs" Inherits="SML.Contact" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main aria-labelledby="title">
        <h2 id="title"><%: Title %>.</h2>
        <p>Hello there!</p>
        <br />
        <p>For any administration issues regarding tournaments and league play, please reach out to the admin running the event.</p>
        <p>If you run into any issues/bugs regarding the website, please reach out through a direct message to Max Snax on discord. Otherwise you can send me an email at the Support account below.</p>
        <br />

       

        <address>
            <strong>Support:</strong>   <a href="mailto:max.snax.contact@gmail.com">max.snax.contact@gmail.com</a><br />
        </address>
    </main>
</asp:Content>
