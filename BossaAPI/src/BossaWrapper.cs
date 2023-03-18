using System;

namespace pjank.BossaAPI.src
{
    public class BossaWrapper
    {
		/// <summary>
		/// Czy jesteśmy połączeni z serwerem (wywołano wcześniej "Connect")?
		/// Jeśli nie, wszelkie operacje które wymagają tego połączenia, zwrócą teraz wyjątek.
		/// </summary>
		public bool Connected => Bossa.Connected;

		/// <summary>
		/// Dostęp do naszych rachunków w biurze maklerskim Bossa
		/// (ich saldo, obecne papiery wartościowe, bieżące zlecenia).
		/// </summary>
		public BosAccounts Accounts => Bossa.Accounts;

		public BosAccount GetAccount(string number) => Bossa.Accounts[number];

		/// <summary>
		/// Dostęp do informacji o notowaniach poszczególnych instrumentów na rynku
		/// (historia ostatnich transakcji, bieżąca tabela ofert kupna/sprzedaży).
		/// </summary>
		public BosInstruments Instruments => Bossa.Instruments;

		public BosInstrument GetInstrument(string symbol) => Bossa.Instruments[symbol];

		/// <summary>
		/// Zdarzenie wywoływane po każdej aktualizacji danych.
		/// Automatycznie przenosi zdarzenie do wątku odbiorcy, jeśli zajdzie taka potrzeba (BeginInvoke).
		/// Jako parametr "source" przekazywany jest obiekt, który uległ zaktualizowaniu
		/// (obiekt klasy "BosAccount" - jeśli zmiana dotyczy stanu rachunku, bieżących zleceń...
		/// albo "BosInstrument" - jeśli nastąpiła aktualizacja notowań dla danego instrumentu).
		/// TODO: Dokładniejsze przekazywanie w argumentach zdarzenia co konkretnie się zmieniło.
		/// </summary>
		public event EventHandler OnUpdate
		{
			add { Bossa.OnUpdate += value; }
			remove { Bossa.OnUpdate -= value; }
		}

		/// <summary>
		/// Podłączenie wskazanego obiektu komunikującego się z serwerem.
		/// </summary>
		/// <param name="client">Obiekt realizujący konkretną formę komunikacji.
		/// Jedyna dostępna na tę chwilę implementacja tego interfejsu to klasa "NolClient".
		/// </param>
		public void Connect(IBosClient client) => Bossa.Connect(client);

		/// <summary>
		/// Otwarcie połączenia z lokalnie uruchomioną aplikacją NOL3 (Bossa).
		/// </summary>
		public void ConnectNOL3() => Bossa.ConnectNOL3();

		/// <summary>
		/// Zamknięcie bieżącego połączenia.
		/// Wszelkie dane (stan rachunku, notowania) jakie zdążyliśmy zebrać, zostają nadal w pamięci...
		/// i można z nich korzystać (tylko odczyt). Aby wyczyścić wszystkie dane, używamy metody "Clear".
		/// </summary>
		public void Disconnect() => Bossa.Disconnect();

		/// <summary>
		/// Wyczyszczenie zebranych dotąd informacji o stanie naszych rachunków, historii notowań itd.
		/// </summary>
		public void Clear() => Bossa.Clear();
	}
}
