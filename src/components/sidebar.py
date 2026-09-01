import streamlit as st
from typing import Dict, Any, List

def render_sidebar(available_coins: List[Dict[str, Any]]) -> Dict[str, Any]:
    """
    Renders sidebar filters and controls.
    Returns a dictionary of selected user preferences.
    """
    st.sidebar.title("🎛️ Filtros & Control")
    st.sidebar.markdown("---")
    
    # Currency selector
    currency = st.sidebar.selectbox(
        "💱 Moneda de Referencia",
        options=["usd", "eur", "gbp"],
        format_func=lambda x: x.upper(),
        index=0
    )
    
    # Coin selector
    coin_options = {coin.get("name", coin.get("id")): coin.get("id") for coin in available_coins}
    selected_coin_name = st.sidebar.selectbox(
        "🪙 Seleccionar Criptomoneda",
        options=list(coin_options.keys()),
        index=0
    )
    selected_coin_id = coin_options[selected_coin_name]
    
    # Time Horizon
    days = st.sidebar.select_slider(
        "📅 Horizonte Temporal (Días)",
        options=[7, 14, 30, 90, 180, 365],
        value=30
    )
    
    st.sidebar.markdown("---")
    st.sidebar.subheader("📈 Indicadores Técnicos")
    show_sma7 = st.sidebar.checkbox("Mostrar SMA (7 días)", value=True)
    show_sma30 = st.sidebar.checkbox("Mostrar SMA (30 días)", value=True)
    show_volume = st.sidebar.checkbox("Mostrar Volumen de Comercio", value=True)
    
    st.sidebar.markdown("---")
    st.sidebar.info(
        "💡 **PRO TIP (CV):** Este panel se conecta a la API REST de CoinGecko, procesa los datos en DataFrames de Pandas y genera gráficos reactivos con Plotly."
    )
    
    return {
        "currency": currency,
        "coin_id": selected_coin_id,
        "coin_name": selected_coin_name,
        "days": days,
        "show_sma7": show_sma7,
        "show_sma30": show_sma30,
        "show_volume": show_volume
    }
