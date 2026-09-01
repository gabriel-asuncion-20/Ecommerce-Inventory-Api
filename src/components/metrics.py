import streamlit as st
import pandas as pd
from src.data_processor import format_currency

def render_kpi_cards(selected_coin_data: dict, history_df: pd.DataFrame, currency: str = "usd"):
    """
    Renders top KPI metric cards with glassmorphism styled layout.
    """
    sym = "$" if currency == "usd" else ("€" if currency == "eur" else "£")
    
    current_price = selected_coin_data.get("current_price", history_df["price"].iloc[-1] if not history_df.empty else 0.0)
    change_24h = selected_coin_data.get("price_change_percentage_24h", 0.0)
    market_cap = selected_coin_data.get("market_cap", 0.0)
    total_vol = selected_coin_data.get("total_volume", history_df["total_volume"].iloc[-1] if not history_df.empty else 0.0)
    
    max_price = history_df["price"].max() if not history_df.empty else current_price
    min_price = history_df["price"].min() if not history_df.empty else current_price

    col1, col2, col3, col4 = st.columns(4)
    
    with col1:
        st.metric(
            label="💵 Precio Actual",
            value=f"{sym}{current_price:,.2f}",
            delta=f"{change_24h:+.2f}% (24h)"
        )
        
    with col2:
        st.metric(
            label="🏛️ Capitalización de Mercado",
            value=format_currency(market_cap, sym),
            delta="Market Cap Rank #" + str(selected_coin_data.get("market_cap_rank", 1))
        )
        
    with col3:
        st.metric(
            label="📊 Volumen Total",
            value=format_currency(total_vol, sym),
            delta=None
        )
        
    with col4:
        volatility = history_df["volatility_7d"].iloc[-1] if "volatility_7d" in history_df.columns and not history_df.empty else 0.0
        st.metric(
            label="⚡ Volatilidad (7d)",
            value=f"{volatility:.2f}%",
            delta=f"Máx: {sym}{max_price:,.2f} | Mín: {sym}{min_price:,.2f}",
            delta_color="off"
        )
