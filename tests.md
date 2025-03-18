## Acceptansvilkor

- Skeppet är på en waypoint (!) framför moderskeppet
  (avdockning är klar)
- Det finns en vektor från startpunkten
- Det finns en orientering som skeppet vill ha
- Skeppet kan orientera sig i orienteringsriktningen
- Skeppet kan röra sig frammåt längs vektorn under bibehållen orientering
- Skeppet håller sin position och orientering på vektorn under nominell yttre påverkan
- Skeppet använder borrar för att gräva ut asteroider i sin väg i frammåt riktningen
- Skeppet anspassar hastigheten för att inte kollidera med asteroider.
- Skeppet åker inte allt för långsamt när det inte finns något att kollidera med
- Skeppet kan återvända till waypointen, längs vektorn
- Skeppet undviker kollisioner när det återvänder.

## Implementationsdetaljer

- Vektorn är förlängningen mellan två waypoints i en docknings AI? Anpassat till en konstant 
  längd
- Skeppet har:
    1. sensorer för att detektera asteroider
    2. docknings ai
    3. thrusters i alla kardinalriktningar
    4. gyro
    5. remote control

## Test

1.) Programmet tar en input:
    om inputen är debug skriv ut ett meddelande i programmets output
2.) Programmet har ett remote block, om det inte finns, skriv ut det vid debug
3.) Det finns en sträng med remoteblockets namn, vid programstart läser programmet in objektet med det namnet. Om det finns, skriv ut att det gör det.
4.) Skriv ut remote control blockets namn när det hittats
5.) Skriv ut remote control blockets lokala quarternion(?)