using System;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using pjank.BossaAPI.DTO;

namespace pjank.BossaAPI.Fixml
{
	public class ExecutionReportMsg : FixmlMsg
	{
		public const string MsgName = "ExecRpt";

		public string BrokerOrderId { get; private set; }           // id zlecenia z DM, *nie* występuje w potw. transakcji
		public string BrokerOrderId2 { get; private set; }          // nr zlecenia, obecny zawsze (przynajmniej docelowo)
		public string ClientOrderId { get; private set; }           // id zlecenia nadawane przez nas
		public string StatusReqId { get; private set; }             // id zapytania o status zlecenia (OrderStatusRequest)
		public string ExecId { get; private set; }                  // id wykonania zlecenia
		public ExecReportType? ExecType { get; private set; }       // typ operacji, która spowodowała wysłanie tego raportu
		public ExecReportStatus? Status { get; private set; }       // aktualny status zlecenia
		public OrdRejectReason? RejectReason { get; private set; }  // powód odrzucenia zlecenia
		public string Account { get; private set; }                 // numer rachunku
		public FixmlInstrument Instrument { get; private set; }     // papier wartościowy, którego dotyczy raport
		public OrderSide Side { get; private set; }                 // 1=kupno, 2=sprzedaż
		public uint? Quantity { get; private set; }                 // ilość papierów w zleceniu
		public OrderType? Type { get; private set; }                // typ zlecenia (limit, PKC, stoploss itp.)
		public decimal? Price { get; private set; }                 // cena zlecenia
		public decimal? StopPrice { get; private set; }             // limit aktywacji
		public string Currency { get; private set; }                // waluta
		public OrdTimeInForce? TimeInForce { get; private set; }    // rodzaj ważności
		public DateTime? ExpireDate { get; private set; }           // data ważności zlecenia
		public decimal? LastPrice { get; private set; }             // cena w ostatniej transakcji
		public uint? LastQuantity { get; private set; }             // ilość w ostatniej transakcji
		public uint? LeavesQuantity { get; private set; }           // ilość pozostała w zleceniu
		public uint? CumulatedQuantity { get; private set; }        // dotychczas zrealizowana ilość
		public DateTime? TransactionTime { get; private set; }      // czas transakcji
		public decimal? Commission { get; private set; }            // wartość prowizji
		public OrdCommissionType? CommissionType { get; private set; }// typ prowizji
		public decimal? NetMoney { get; private set; }              // wartość netto transakcji
		public uint? MinimumQuantity { get; private set; }          // ilość minimalna
		public uint? DisplayQuantity { get; private set; }          // ilość ujawniona
		public string Text { get; private set; }                    // dowolny tekst
		public char? TriggerType { get; private set; }              // rodzaj triggera (4 - DDM+ po zmianie ceny)
		public char? TriggerAction { get; private set; }            // akcja triggera (1 - aktywacja zlecenia DDM+)
		public decimal? TriggerPrice { get; private set; }          // cena uaktywnienia triggera (DDM+)
		public char? TriggerPriceType { get; private set; }         // rodzaj ceny uakt. (2 = cena ost. transakcji)
		public char? DeferredPaymentType { get; private set; }      // OTP (odroczony termin płatności) = T/P

		public ExecutionReportMsg(Socket s) : base(s) { }
		public ExecutionReportMsg(FixmlMsg m) : base(m) { }

		protected override void ParseXmlMessage(string name)
		{
			base.ParseXmlMessage(MsgName);
			BrokerOrderId = FixmlUtil.ReadString(xml, "OrdID", true);
			BrokerOrderId2 = FixmlUtil.ReadString(xml, "OrdID2", true);
			ClientOrderId = FixmlUtil.ReadString(xml, "ID", true);
			StatusReqId = FixmlUtil.ReadString(xml, "StatReqID", true);
			ExecId = FixmlUtil.ReadString(xml, "ExecID");
			ExecType = ExecRptTypeUtil.Read(xml, "ExecTyp", true);
			Status = ExecRptStatUtil.Read(xml, "Stat", true);
			RejectReason = OrderRejRsnUtil.Read(xml, "RejRsn", true);
			Account = FixmlUtil.ReadString(xml, "Acct", true);
			Instrument = FixmlInstrument.Read(xml, "Instrmt");
			Side = OrderSideUtil.Read(xml, "Side");
			Quantity = FixmlUtil.ReadUInt(xml, "OrdQty/Qty", true);
			Type = OrderTypeUtil.Read(xml, "OrdTyp", true);
			Price = FixmlUtil.ReadDecimal(xml, "Px", true);
			StopPrice = FixmlUtil.ReadDecimal(xml, "StopPx", true);
			Currency = FixmlUtil.ReadString(xml, "Ccy", true);
			TimeInForce = OrdTmInForceUtil.Read(xml, "TmInForce");
			ExpireDate = FixmlUtil.ReadDateTime(xml, "ExpireDt", true);
			LastPrice = FixmlUtil.ReadDecimal(xml, "LastPx", true);
			LastQuantity = FixmlUtil.ReadUInt(Xml, "LastQty", true);
			LeavesQuantity = FixmlUtil.ReadUInt(xml, "LeavesQty", true);
			CumulatedQuantity = FixmlUtil.ReadUInt(xml, "CumQty", true);
			TransactionTime = FixmlUtil.ReadDateTime(xml, "TxnTm", true);
			Commission = FixmlUtil.ReadDecimal(xml, "Comm/Comm", true);
			CommissionType = OrdCommTypeUtil.Read(xml, "Comm/CommTyp", true);
			NetMoney = FixmlUtil.ReadDecimal(xml, "NetMny", true);
			MinimumQuantity = FixmlUtil.ReadUInt(xml, "MinQty", true);
			DisplayQuantity = FixmlUtil.ReadUInt(xml, "DsplyInstr/DisplayQty", true);
			Text = FixmlUtil.ReadString(xml, "Text", true);
			TriggerType = FixmlUtil.ReadChar(xml, "TrgrInstr/TrgrTyp", true);
			TriggerAction = FixmlUtil.ReadChar(xml, "TrgrInstr/TrgrActn", true);
			TriggerPrice = FixmlUtil.ReadDecimal(xml, "TrgrInstr/TrgrPx", true);
			TriggerPriceType = FixmlUtil.ReadChar(xml, "TrgrInstr/TrgrPxTyp", true);
			DeferredPaymentType = FixmlUtil.ReadChar(xml, "DefPayTyp", true);
		}

		public string PriceStr
		{
			get
			{
				if ((Type == OrderType.PKC) || (Type == OrderType.StopLoss)) return "PKC";
				if (Type == OrderType.PCR_PCRO)
					switch (TimeInForce)
					{
						case OrdTimeInForce.Opening:
						case OrdTimeInForce.Closing: return "PCRO";
						default: return "PCR";
					}
				return Price.ToString();
			}
		}

		public decimal? CommissionValue
		{
			get
			{
				switch (CommissionType)
				{
					case OrdCommissionType.PerUnit: return Commission * Quantity;  // LastQuantity?
					case OrdCommissionType.Percent: return Commission * NetMoney;
					case OrdCommissionType.Absolute: return Commission;
					default: return null;
				}
			}
		}

		public OrderData GetOrderDate()
        {
			var order = new OrderData();
			order.AccountNumber = Account;
			order.BrokerId = BrokerOrderId2;
			order.ClientId = ClientOrderId;

			// UWAGA: odczyt danych z komunikatu ExecRpt odbywa się "na czuja"... bo dostarczoną 
			// z Bossy/Comarchu dokumentacją na ten temat to można sobie najwyżej w kominku podpalić.
			// Generalnie wszystko jest rozjechane, ale te statusy zleceń to już chyba po pijaku pisali.
			//   Tyle - musiałem to tu napisać !!!  ;-P
			// Bo mnie już krew zalewa, jak widzę co ten NOL3 wysyła i gdzie (w których polach)... 
			// I że w ogóle musiałem te wszystkie możliwe przypadki samodzielnie analizować...
			// A jak za miesiąc przestanie działać, bo coś tam "naprawią", to chyba kogoś postrzelę ;-(


			// raport o wykonaniu - być może cząstkowym - danego zlecenia
			// (z praktyki wynika, że zawsze wcześniej dostaniemy "pełen" ExecRpt z pozostałymi
			// informacjami o tym zleceniu... dlatego teraz odbieramy sobie tylko raport z tej transakcji)
			if (ExecType == ExecReportType.Trade)
			{
				order.TradeReport = new OrderTradeData();
				order.TradeReport.Time = (DateTime)TransactionTime;
				order.TradeReport.Price = (decimal)Price;  // LastPrice !?
				order.TradeReport.Quantity = (uint)Quantity;  // LastQuantity !?
				order.TradeReport.NetValue = (decimal)NetMoney;
				order.TradeReport.Commission = (decimal)CommissionValue;
			}
			else
			{
				// w pozostałych przypadkach wygląda na to, że lepiej się oprzeć na polu "Status"
				// (bo ExecType czasem jest, czasem nie ma - różnie to z nim bywa... a Status jest chyba zawsze)
				order.StatusReport = new OrderStatusData();
				order.StatusReport.Status = ExecReport_GetStatus();
				order.StatusReport.Quantity = (uint)CumulatedQuantity;
				order.StatusReport.NetValue = (decimal)NetMoney;
				order.StatusReport.Commission = (decimal)CommissionValue;  // czasem == 0, ale dlaczego!? kto to wie... 

				// pozostałe dane - żeby się nie rozdrabniać - też aktualizujemy za każdym razem
				// (teoretycznie wystarczyłoby przy "new" i "replace"... ale czasem jako pierwsze
				// przychodzi np. "filled" i kto wie co jeszcze innego, więc tak będzie bezpieczniej)
				order.MainData = new OrderMainData();
				order.MainData.CreateTime = (DateTime)TransactionTime;
				order.MainData.Instrument = Instrument.Convert();
				order.MainData.Side = (Side == Fixml.OrderSide.Buy) ? BosOrderSide.Buy : BosOrderSide.Sell;
				order.MainData.PriceType = ExecReport_GetPriceType();
				if (order.MainData.PriceType == PriceType.Limit)
					order.MainData.PriceLimit = Price;
				if ((Type == OrderType.StopLimit) || (Type == OrderType.StopLoss))
					order.MainData.ActivationPrice = StopPrice;
				order.MainData.Quantity = (uint)Quantity;
				order.MainData.MinimumQuantity = (TimeInForce == OrdTimeInForce.WuA) ? Quantity : MinimumQuantity;
				order.MainData.VisibleQuantity = DisplayQuantity;
				order.MainData.ImmediateOrCancel = (TimeInForce == OrdTimeInForce.WiA);
				order.MainData.ExpirationDate = (TimeInForce == OrdTimeInForce.Date) ? ExpireDate : null;
			}

			return order;
		}

		// funkcja pomocnicza zamieniająca status zlecenia FIXML na nasz BosOrderStatus
		private BosOrderStatus ExecReport_GetStatus()
		{
			switch (Status)
			{
				case ExecReportStatus.New: return BosOrderStatus.Active;
				case ExecReportStatus.PartiallyFilled: return BosOrderStatus.ActiveFilled;
				case ExecReportStatus.Canceled: return ((CumulatedQuantity ?? 0) > 0) ? BosOrderStatus.CancelledFilled : BosOrderStatus.Cancelled;
				case ExecReportStatus.Filled: return BosOrderStatus.Filled;
				case ExecReportStatus.Expired: return BosOrderStatus.Expired;
				case ExecReportStatus.Rejected: return BosOrderStatus.Rejected;
				case ExecReportStatus.PendingReplace: return BosOrderStatus.PendingReplace;
				case ExecReportStatus.PendingCancel: return BosOrderStatus.PendingCancel;
				default: throw new ArgumentException("Unknown ExecReport-Status");
			}
		}

		// funkcja pomocnicza zamieniająca typ zlecenia FIXML na nasz DTO.PriceType
		private PriceType ExecReport_GetPriceType()
		{
			switch (Type)
			{
				case OrderType.Limit:
				case OrderType.StopLimit: return PriceType.Limit;
				case OrderType.PKC:
				case OrderType.StopLoss: return PriceType.PKC;
				case OrderType.PCR_PCRO:
					var time = TimeInForce;
					var pcro = ((time == OrdTimeInForce.Opening) || (time == OrdTimeInForce.Closing));
					return pcro ? PriceType.PCRO : PriceType.PCR;
				default: throw new ArgumentException("Unknown ExecReport-OrderType");
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(string.Format("[{0}:{1}:{2}] {3:T}", Xml.Name, BrokerOrderId2, ClientOrderId, TransactionTime));
			sb.Append(string.Format("  {0}  {1,-4} {2} x {3,-7}", Instrument, Side, Quantity, PriceStr));
			if (StopPrice != null) sb.Append(string.Format(" @{0}", StopPrice));
			if (Status != null) sb.Append(string.Format("  {0}", Status));
			else
				if (ExecType != null) sb.Append(string.Format("  ({0})", ExecType));
			if (Text != null) sb.Append(string.Format("'{0}'", Text));
			return sb.ToString();
		}

	}
}
