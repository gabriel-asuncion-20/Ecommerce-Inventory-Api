import plotly.graph_objects as go
import plotly.express as px
import pandas as pd
import streamlit as st

PRIMARY_COLOR = "#8B5CF6"
SECONDARY_COLOR = "#06B6D4"
ACCENT_COLOR = "#F59E0B"
DARK_BG = "#0F172A"
CARD_BG = "#1E293B"

def render_price_chart(df: pd.DataFrame, coin_name: str, settings: dict):
    """
    Renders interactive main price chart with technical indicators (SMA_7, SMA_30).
    """
    fig = go.Figure()
    
    # Main price line
    fig.add_trace(go.Scatter(
        x=df["timestamp"],
        y=df["price"],
        mode="lines",
        name=f"Precio {coin_name}",
        line=dict(color=PRIMARY_COLOR, width=2.5),
        hovertemplate="<b>Fecha:</b> %{x|%d %b %Y}<br><b>Precio:</b> $%{y:,.2f}<extra></extra>"
    ))
    
    # Technical Indicators
    if settings.get("show_sma7") and "SMA_7" in df.columns:
        fig.add_trace(go.Scatter(
            x=df["timestamp"],
            y=df["SMA_7"],
            mode="lines",
            name="SMA (7 días)",
            line=dict(color=SECONDARY_COLOR, width=1.5, dash="dash"),
            hovertemplate="<b>SMA 7:</b> $%{y:,.2f}<extra></extra>"
        ))
        
    if settings.get("show_sma30") and "SMA_30" in df.columns:
        fig.add_trace(go.Scatter(
            x=df["timestamp"],
            y=df["SMA_30"],
            mode="lines",
            name="SMA (30 días)",
            line=dict(color=ACCENT_COLOR, width=1.5, dash="dot"),
            hovertemplate="<b>SMA 30:</b> $%{y:,.2f}<extra></extra>"
        ))
        
    fig.update_layout(
        title=f"📈 Evolución del Precio: {coin_name}",
        template="plotly_dark",
        paper_bgcolor=CARD_BG,
        plot_bgcolor=DARK_BG,
        margin=dict(l=20, r=20, t=50, b=20),
        xaxis=dict(gridcolor="#334155", title="Fecha"),
        yaxis=dict(gridcolor="#334155", title=f"Precio ({settings.get('currency', 'usd').upper()})"),
        legend=dict(orientation="h", yanchor="bottom", y=1.02, xanchor="right", x=1)
    )
    
    st.plotly_chart(fig, use_container_width=True)


def render_volume_chart(df: pd.DataFrame, coin_name: str):
    """
    Renders bar chart for market trading volume.
    """
    fig = px.bar(
        df,
        x="timestamp",
        y="total_volume",
        title=f"📊 Volumen Diario de Comercio: {coin_name}",
        labels={"timestamp": "Fecha", "total_volume": "Volumen Total ($)"},
        template="plotly_dark"
    )
    
    fig.update_traces(marker_color=SECONDARY_COLOR, opacity=0.75)
    fig.update_layout(
        paper_bgcolor=CARD_BG,
        plot_bgcolor=DARK_BG,
        margin=dict(l=20, r=20, t=50, b=20),
        xaxis=dict(gridcolor="#334155"),
        yaxis=dict(gridcolor="#334155")
    )
    
    st.plotly_chart(fig, use_container_width=True)


def render_market_cap_distribution(top_coins_df: pd.DataFrame):
    """
    Renders donut pie chart showing market cap dominance among top assets.
    """
    fig = px.pie(
        top_coins_df,
        values="market_cap",
        names="name",
        title="🥧 Dominancia por Capitalización de Mercado",
        hole=0.4,
        color_discrete_sequence=px.colors.qualitative.Pastel
    )
    
    fig.update_layout(
        template="plotly_dark",
        paper_bgcolor=CARD_BG,
        margin=dict(l=20, r=20, t=50, b=20)
    )
    
    st.plotly_chart(fig, use_container_width=True)


def render_top_coins_comparison(top_coins_df: pd.DataFrame):
    """
    Renders bar chart comparing 24h price percentage changes across top coins.
    """
    colors = ["#10B981" if val >= 0 else "#EF4444" for val in top_coins_df["price_change_percentage_24h"]]
    
    fig = go.Figure(go.Bar(
        x=top_coins_df["symbol"],
        y=top_coins_df["price_change_percentage_24h"],
        marker_color=colors,
        text=[f"{val:+.2f}%" for val in top_coins_df["price_change_percentage_24h"]],
        textposition="outside"
    ))
    
    fig.update_layout(
        title="⚡ Variación del Precio en 24h (%) - Principales Activos",
        template="plotly_dark",
        paper_bgcolor=CARD_BG,
        plot_bgcolor=DARK_BG,
        margin=dict(l=20, r=20, t=50, b=20),
        xaxis=dict(gridcolor="#334155", title="Criptomoneda"),
        yaxis=dict(gridcolor="#334155", title="Cambio %")
    )
    
    st.plotly_chart(fig, use_container_width=True)
