using System.Collections.Generic;

namespace ContaFlow.API.Data
{
    public static class HondurasGeoData
    {
        public class DepDto
        {
            public string Codigo { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string Cabecera { get; set; } = string.Empty;
            public List<MunDto> Municipios { get; set; } = new();
        }

        public class MunDto
        {
            public string Codigo { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }

        public static List<DepDto> ObtenerCatalogoCompleto()
        {
            return new List<DepDto>
            {
                new DepDto {
                    Codigo = "01", Nombre = "Atlántida", Cabecera = "La Ceiba",
                    Municipios = new List<MunDto> {
                        new MunDto { Codigo = "0101", Nombre = "La Ceiba" },
                        new MunDto { Codigo = "0102", Nombre = "El Porvenir" },
                        new MunDto { Codigo = "0103", Nombre = "Esparta" },
                        new MunDto { Codigo = "0104", Nombre = "Jutiapa" },
                        new MunDto { Codigo = "0105", Nombre = "La Masica" },
                        new MunDto { Codigo = "0106", Nombre = "San Francisco" },
                        new MunDto { Codigo = "0107", Nombre = "Tela" },
                        new MunDto { Codigo = "0108", Nombre = "Arizona" }
                    }
                },
                new DepDto {
                    Codigo = "02", Nombre = "Colón", Cabecera = "Trujillo",
                    Municipios = new List<MunDto> {
                        new MunDto { Codigo = "0201", Nombre = "Trujillo" },
                        new MunDto { Codigo = "0202", Nombre = "Balfate" },
                        new MunDto { Codigo = "0203", Nombre = "Iriona" },
                        new MunDto { Codigo = "0204", Nombre = "Limón" },
                        new MunDto { Codigo = "0205", Nombre = "Sabá" },
                        new MunDto { Codigo = "0206", Nombre = "Santa Fe" },
                        new MunDto { Codigo = "0207", Nombre = "Santa Rosa de Aguán" },
                        new MunDto { Codigo = "0208", Nombre = "Sonaguera" },
                        new MunDto { Codigo = "0209", Nombre = "Tocoa" },
                        new MunDto { Codigo = "0210", Nombre = "Bonito Oriental" }
                    }
                },
                new DepDto {
                    Codigo = "03", Nombre = "Comayagua", Cabecera = "Comayagua",
                    Municipios = new List<MunDto> {
                        new MunDto { Codigo = "0301", Nombre = "Comayagua" },
                        new MunDto { Codigo = "0302", Nombre = "Ajuterique" },
                        new MunDto { Codigo = "0303", Nombre = "El Rosario" },
                        new MunDto { Codigo = "0304", Nombre = "Esquías" },
                        new MunDto { Codigo = "0305", Nombre = "Humuza" },
                        new MunDto { Codigo = "0306", Nombre = "La Libertad" },
                        new MunDto { Codigo = "0307", Nombre = "Lamaní" },
                        new MunDto { Codigo = "0308", Nombre = "La Trinidad" },
                        new MunDto { Codigo = "0309", Nombre = "Lejamaní" },
                        new MunDto { Codigo = "0310", Nombre = "Meámbar" },
                        new MunDto { Codigo = "0311", Nombre = "Minas de Oro" },
                        new MunDto { Codigo = "0312", Nombre = "Ojos de Agua" },
                        new MunDto { Codigo = "0313", Nombre = "San Jerónimo" },
                        new MunDto { Codigo = "0314", Nombre = "San José de Comayagua" },
                        new MunDto { Codigo = "0315", Nombre = "San José del Potrero" },
                        new MunDto { Codigo = "0316", Nombre = "San Luis" },
                        new MunDto { Codigo = "0317", Nombre = "San Sebastián" },
                        new MunDto { Codigo = "0318", Nombre = "Siguatepeque" },
                        new MunDto { Codigo = "0319", Nombre = "Villa de San Antonio" },
                        new MunDto { Codigo = "0320", Nombre = "Las Lajas" },
                        new MunDto { Codigo = "0321", Nombre = "Taulabé" }
                    }
                },
                new DepDto {
                    Codigo = "04", Nombre = "Copán", Cabecera = "Santa Rosa de Copán",
                    Municipios = new List<MunDto> {
                        new MunDto { Codigo = "0401", Nombre = "Santa Rosa de Copán" },
                        new MunDto { Codigo = "0402", Nombre = "Cabañas" },
                        new MunDto { Codigo = "0403", Nombre = "Concepción" },
                        new MunDto { Codigo = "0404", Nombre = "Copán Ruinas" },
                        new MunDto { Codigo = "0405", Nombre = "Corquín" },
                        new MunDto { Codigo = "0406", Nombre = "Cucuyagua" },
                        new MunDto { Codigo = "0407", Nombre = "Dolores" },
                        new MunDto { Codigo = "0408", Nombre = "Dulce Nombre" },
                        new MunDto { Codigo = "0409", Nombre = "El Paraíso" },
                        new MunDto { Codigo = "0410", Nombre = "Florida" },
                        new MunDto { Codigo = "0411", Nombre = "La Jigua" },
                        new MunDto { Codigo = "0412", Nombre = "La Unión" },
                        new MunDto { Codigo = "0413", Nombre = "Nueva Arcadia (La Entrada)" },
                        new MunDto { Codigo = "0414", Nombre = "San Agustín" },
                        new MunDto { Codigo = "0415", Nombre = "San Antonio" },
                        new MunDto { Codigo = "0416", Nombre = "San Jerónimo" },
                        new MunDto { Codigo = "0417", Nombre = "San José" },
                        new MunDto { Codigo = "0418", Nombre = "San Juan de Opoa" },
                        new MunDto { Codigo = "0419", Nombre = "San Nicolás" },
                        new MunDto { Codigo = "0420", Nombre = "San Pedro" },
                        new MunDto { Codigo = "0421", Nombre = "Santa Rita" },
                        new MunDto { Codigo = "0422", Nombre = "Trinidad de Copán" },
                        new MunDto { Codigo = "0423", Nombre = "Veracruz" }
                    }
                },
                new DepDto {
                    Codigo = "05", Nombre = "Cortés", Cabecera = "San Pedro Sula",
                    Municipios = new List<MunDto> {
                        new MunDto { Codigo = "0501", Nombre = "San Pedro Sula" },
                        new MunDto { Codigo = "0502", Nombre = "Choloma" },
                        new MunDto { Codigo = "0503", Nombre = "Omoa" },
                        new MunDto { Codigo = "0504", Nombre = "Pimienta" },
                        new MunDto { Codigo = "0505", Nombre = "Potrerillos" },
                        new MunDto { Codigo = "0506", Nombre = "Puerto Cortés" },
                        new MunDto { Codigo = "0507", Nombre = "San Antonio de Cortés" },
                        new MunDto { Codigo = "0508", Nombre = "San Francisco de Yojoa" },
                        new MunDto { Codigo = "0509", Nombre = "San Manuel" },
                        new MunDto { Codigo = "0510", Nombre = "Santa Cruz de Yojoa" },
                        new MunDto { Codigo = "0511", Nombre = "Villanueva" },
                        new MunDto { Codigo = "0512", Nombre = "La Lima" }
                    }
                },
                new DepDto {
                    Codigo = "06", Nombre = "Choluteca", Cabecera = "Choluteca",
                    Municipios = new List<MunDto> {
                        new MunDto { Codigo = "0601", Nombre = "Choluteca" },
                        new MunDto { Codigo = "0602", Nombre = "Apacilagua" },
                        new MunDto { Codigo = "0603", Nombre = "Concepción de María" },
                        new MunDto { Codigo = "0604", Nombre = "Duyure" },
                        new MunDto { Codigo = "0605", Nombre = "El Corpus" },
                        new MunDto { Codigo = "0606", Nombre = "El Triunfo" },
                        new MunDto { Codigo = "0607", Nombre = "Marcovia" },
                        new MunDto { Codigo = "0608", Nombre = "Morolica" },
                        new MunDto { Codigo = "0609", Nombre = "Namasigüe" },
                        new MunDto { Codigo = "0610", Nombre = "Orocuina" },
                        new MunDto { Codigo = "0611", Nombre = "Pespire" },
                        new MunDto { Codigo = "0612", Nombre = "San Antonio de Flores" },
                        new MunDto { Codigo = "0613", Nombre = "San Isidro" },
                        new MunDto { Codigo = "0614", Nombre = "San José" },
                        new MunDto { Codigo = "0615", Nombre = "San Marcos de Colón" },
                        new MunDto { Codigo = "0616", Nombre = "Santa Ana de Yusguare" }
                    }
                },
                new DepDto {
                    Codigo = "07", Nombre = "El Paraíso", Cabecera = "Yuscarán",
                    Municipios = new List<MunDto> {
                        new MunDto { Codigo = "0701", Nombre = "Yuscarán" },
                        new MunDto { Codigo = "0702", Nombre = "Alauca" },
                        new MunDto { Codigo = "0703", Nombre = "Danlí" },
                        new MunDto { Codigo = "0704", Nombre = "El Paraíso" },
                        new MunDto { Codigo = "0705", Nombre = "Güinope" },
                        new MunDto { Codigo = "0706", Nombre = "Jacaleapa" },
                        new MunDto { Codigo = "0707", Nombre = "Liure" },
                        new MunDto { Codigo = "0708", Nombre = "Morocelí" },
                        new MunDto { Codigo = "0709", Nombre = "Oropolí" },
                        new MunDto { Codigo = "0710", Nombre = "Potrerillos" },
                        new MunDto { Codigo = "0711", Nombre = "San Antonio de Flores" },
                        new MunDto { Codigo = "0712", Nombre = "San Lucas" },
                        new MunDto { Codigo = "0713", Nombre = "San Matías" },
                        new MunDto { Codigo = "0714", Nombre = "Soledad" },
                        new MunDto { Codigo = "0715", Nombre = "Teupasenti" },
                        new MunDto { Codigo = "0716", Nombre = "Texiguat" },
                        new MunDto { Codigo = "0717", Nombre = "Vado Ancho" },
                        new MunDto { Codigo = "0718", Nombre = "Yauyupe" },
                        new MunDto { Codigo = "0719", Nombre = "Trojes" }
                    }
                },
                new DepDto {
                    Codigo = "08", Nombre = "Francisco Morazán", Cabecera = "Distrito Central",
                    Municipios = new List<MunDto> {
                        new MunDto { Codigo = "0801", Nombre = "Distrito Central (Tegucigalpa / M.D.C.)" },
                        new MunDto { Codigo = "0802", Nombre = "Alubarén" },
                        new MunDto { Codigo = "0803", Nombre = "Cedros" },
                        new MunDto { Codigo = "0804", Nombre = "Curarén" },
                        new MunDto { Codigo = "0805", Nombre = "El Porvenir" },
                        new MunDto { Codigo = "0806", Nombre = "Guaimaca" },
                        new MunDto { Codigo = "0807", Nombre = "La Libertad" },
                        new MunDto { Codigo = "0808", Nombre = "La Venta" },
                        new MunDto { Codigo = "0809", Nombre = "Lepaterique" },
                        new MunDto { Codigo = "0810", Nombre = "Maraita" },
                        new MunDto { Codigo = "0811", Nombre = "Marale" },
                        new MunDto { Codigo = "0812", Nombre = "Nueva Armenia" },
                        new MunDto { Codigo = "0813", Nombre = "Ojojona" },
                        new MunDto { Codigo = "0814", Nombre = "Orica" },
                        new MunDto { Codigo = "0815", Nombre = "Reitoca" },
                        new MunDto { Codigo = "0816", Nombre = "Sabanagrande" },
                        new MunDto { Codigo = "0817", Nombre = "San Antonio de Oriente" },
                        new MunDto { Codigo = "0818", Nombre = "San Buenaventura" },
                        new MunDto { Codigo = "0819", Nombre = "San Ignacio" },
                        new MunDto { Codigo = "0820", Nombre = "San Juan de Flores (Cantarranas)" },
                        new MunDto { Codigo = "0821", Nombre = "San Miguelito" },
                        new MunDto { Codigo = "0822", Nombre = "Santa Ana" },
                        new MunDto { Codigo = "0823", Nombre = "Santa Lucía" },
                        new MunDto { Codigo = "0824", Nombre = "Talanga" },
                        new MunDto { Codigo = "0825", Nombre = "Tatumbla" },
                        new MunDto { Codigo = "0826", Nombre = "Valle de Ángeles" },
                        new MunDto { Codigo = "0827", Nombre = "Villa de San Francisco" },
                        new MunDto { Codigo = "0828", Nombre = "Vallecillo" }
                    }
                },
                new DepDto {
                    Codigo = "09", Nombre = "Gracias a Dios", Cabecera = "Puerto Lempira",
                    Municipios = new List<MunDto> {
                        new MunDto { Codigo = "0901", Nombre = "Puerto Lempira" },
                        new MunDto { Codigo = "0902", Nombre = "Brus Laguna" },
                        new MunDto { Codigo = "0903", Nombre = "Ahuas" },
                        new MunDto { Codigo = "0904", Nombre = "Juan Francisco Bulnes" },
                        new MunDto { Codigo = "0905", Nombre = "Ramón Villeda Morales" },
                        new MunDto { Codigo = "0906", Nombre = "Wampusirpi" }
                    }
                },
                new DepDto {
                    Codigo = "10", Nombre = "Intibucá", Cabecera = "La Esperanza",
                    Municipios = new List<MunDto> {
                        new MunDto { Codigo = "1001", Nombre = "La Esperanza" },
                        new MunDto { Codigo = "1002", Nombre = "Camasca" },
                        new MunDto { Codigo = "1003", Nombre = "Colomoncagua" },
                        new MunDto { Codigo = "1004", Nombre = "Concepción" },
                        new MunDto { Codigo = "1005", Nombre = "Dolores" },
                        new MunDto { Codigo = "1006", Nombre = "Intibucá" },
                        new MunDto { Codigo = "1007", Nombre = "Jesús de Otoro" },
                        new MunDto { Codigo = "1008", Nombre = "Magdalena" },
                        new MunDto { Codigo = "1009", Nombre = "Masaguara" },
                        new MunDto { Codigo = "1010", Nombre = "San Antonio" },
                        new MunDto { Codigo = "1011", Nombre = "San Isidro" },
                        new MunDto { Codigo = "1012", Nombre = "San Juan" },
                        new MunDto { Codigo = "1013", Nombre = "San Marcos de la Sierra" },
                        new MunDto { Codigo = "1014", Nombre = "San Miguel Guancapla" },
                        new MunDto { Codigo = "1015", Nombre = "Santa Lucía" },
                        new MunDto { Codigo = "1016", Nombre = "Yamaranguila" },
                        new MunDto { Codigo = "1017", Nombre = "San Francisco de Opalaca" }
                    }
                },
                new DepDto {
                    Codigo = "11", Nombre = "Islas de la Bahía", Cabecera = "Roatán",
                    Municipios = new List<MunDto> {
                        new MunDto { Codigo = "1101", Nombre = "Roatán" },
                        new MunDto { Codigo = "1102", Nombre = "Guanaja" },
                        new MunDto { Codigo = "1103", Nombre = "José Santos Guardiola" },
                        new MunDto { Codigo = "1104", Nombre = "Utila" }
                    }
                },
                new DepDto {
                    Codigo = "12", Nombre = "La Paz", Cabecera = "La Paz",
                    Municipios = new List<MunDto> {
                        new MunDto { Codigo = "1201", Nombre = "La Paz" },
                        new MunDto { Codigo = "1202", Nombre = "Aguanqueterique" },
                        new MunDto { Codigo = "1203", Nombre = "Cabañas" },
                        new MunDto { Codigo = "1204", Nombre = "Cane" },
                        new MunDto { Codigo = "1205", Nombre = "Chinacla" },
                        new MunDto { Codigo = "1206", Nombre = "Guajiquiro" },
                        new MunDto { Codigo = "1207", Nombre = "Lauterique" },
                        new MunDto { Codigo = "1208", Nombre = "Marcala" },
                        new MunDto { Codigo = "1209", Nombre = "Mercedes de Oriente" },
                        new MunDto { Codigo = "1210", Nombre = "Opatoro" },
                        new MunDto { Codigo = "1211", Nombre = "San Antonio del Norte" },
                        new MunDto { Codigo = "1212", Nombre = "San José" },
                        new MunDto { Codigo = "1213", Nombre = "San Juan" },
                        new MunDto { Codigo = "1214", Nombre = "San Pedro de Tutule" },
                        new MunDto { Codigo = "1215", Nombre = "Santa Ana" },
                        new MunDto { Codigo = "1216", Nombre = "Santa Elena" },
                        new MunDto { Codigo = "1217", Nombre = "Santa María" },
                        new MunDto { Codigo = "1218", Nombre = "Santiago de Puringla" },
                        new MunDto { Codigo = "1219", Nombre = "Yarula" }
                    }
                },
                new DepDto {
                    Codigo = "13", Nombre = "Lempira", Cabecera = "Gracias",
                    Municipios = new List<MunDto> {
                        new MunDto { Codigo = "1301", Nombre = "Gracias" },
                        new MunDto { Codigo = "1302", Nombre = "Belén" },
                        new MunDto { Codigo = "1303", Nombre = "Candelaria" },
                        new MunDto { Codigo = "1304", Nombre = "Cololaca" },
                        new MunDto { Codigo = "1305", Nombre = "Erandique" },
                        new MunDto { Codigo = "1306", Nombre = "Gualcince" },
                        new MunDto { Codigo = "1307", Nombre = "Guarita" },
                        new MunDto { Codigo = "1308", Nombre = "La Campa" },
                        new MunDto { Codigo = "1309", Nombre = "La Iguala" },
                        new MunDto { Codigo = "1310", Nombre = "Las Flores" },
                        new MunDto { Codigo = "1311", Nombre = "La Unión" },
                        new MunDto { Codigo = "1312", Nombre = "Mapulaca" },
                        new MunDto { Codigo = "1313", Nombre = "Piraera" },
                        new MunDto { Codigo = "1314", Nombre = "San Andrés" },
                        new MunDto { Codigo = "1315", Nombre = "San Francisco" },
                        new MunDto { Codigo = "1316", Nombre = "San Juan Guarita" },
                        new MunDto { Codigo = "1317", Nombre = "San Manuel Colohete" },
                        new MunDto { Codigo = "1318", Nombre = "San Rafael" },
                        new MunDto { Codigo = "1319", Nombre = "San Sebastián" },
                        new MunDto { Codigo = "1320", Nombre = "Santa Cruz" },
                        new MunDto { Codigo = "1321", Nombre = "Talgua" },
                        new MunDto { Codigo = "1322", Nombre = "Tambla" },
                        new MunDto { Codigo = "1323", Nombre = "Tomalá" },
                        new MunDto { Codigo = "1324", Nombre = "Valladolid" },
                        new MunDto { Codigo = "1325", Nombre = "Virginia" },
                        new MunDto { Codigo = "1326", Nombre = "San Marcos de Caiquín" },
                        new MunDto { Codigo = "1327", Nombre = "La Virtud" },
                        new MunDto { Codigo = "1328", Nombre = "Lepaera" }
                    }
                },
                new DepDto {
                    Codigo = "14", Nombre = "Ocotepeque", Cabecera = "Ocotepeque",
                    Municipios = new List<MunDto> {
                        new MunDto { Codigo = "1401", Nombre = "Ocotepeque" },
                        new MunDto { Codigo = "1402", Nombre = "Belén Gualcho" },
                        new MunDto { Codigo = "1403", Nombre = "Concepción" },
                        new MunDto { Codigo = "1404", Nombre = "Dolores Merendón" },
                        new MunDto { Codigo = "1405", Nombre = "Fraternidad" },
                        new MunDto { Codigo = "1406", Nombre = "La Encarnación" },
                        new MunDto { Codigo = "1407", Nombre = "La Labor" },
                        new MunDto { Codigo = "1408", Nombre = "Lucerna" },
                        new MunDto { Codigo = "1409", Nombre = "Mercedes" },
                        new MunDto { Codigo = "1410", Nombre = "San Fernando" },
                        new MunDto { Codigo = "1411", Nombre = "San Francisco del Valle" },
                        new MunDto { Codigo = "1412", Nombre = "San Jorge" },
                        new MunDto { Codigo = "1413", Nombre = "San Marcos" },
                        new MunDto { Codigo = "1414", Nombre = "Santa Fe" },
                        new MunDto { Codigo = "1415", Nombre = "Sensenti" },
                        new MunDto { Codigo = "1416", Nombre = "Sinuapa" }
                    }
                },
                new DepDto {
                    Codigo = "15", Nombre = "Olancho", Cabecera = "Juticalpa",
                    Municipios = new List<MunDto> {
                        new MunDto { Codigo = "1501", Nombre = "Juticalpa" },
                        new MunDto { Codigo = "1502", Nombre = "Campamento" },
                        new MunDto { Codigo = "1503", Nombre = "Catacamas" },
                        new MunDto { Codigo = "1504", Nombre = "Concordia" },
                        new MunDto { Codigo = "1505", Nombre = "Dulce Nombre de Culmí" },
                        new MunDto { Codigo = "1506", Nombre = "El Rosario" },
                        new MunDto { Codigo = "1507", Nombre = "Esquipulas del Norte" },
                        new MunDto { Codigo = "1508", Nombre = "Gualaco" },
                        new MunDto { Codigo = "1509", Nombre = "Guarizama" },
                        new MunDto { Codigo = "1510", Nombre = "Guata" },
                        new MunDto { Codigo = "1511", Nombre = "Guayape" },
                        new MunDto { Codigo = "1512", Nombre = "Jano" },
                        new MunDto { Codigo = "1513", Nombre = "La Unión" },
                        new MunDto { Codigo = "1514", Nombre = "Mangulile" },
                        new MunDto { Codigo = "1515", Nombre = "Manto" },
                        new MunDto { Codigo = "1516", Nombre = "Salamá" },
                        new MunDto { Codigo = "1517", Nombre = "San Esteban" },
                        new MunDto { Codigo = "1518", Nombre = "San Francisco de Becerra" },
                        new MunDto { Codigo = "1519", Nombre = "San Francisco de la Paz" },
                        new MunDto { Codigo = "1520", Nombre = "Santa María del Real" },
                        new MunDto { Codigo = "1521", Nombre = "Silca" },
                        new MunDto { Codigo = "1522", Nombre = "Yocón" },
                        new MunDto { Codigo = "1523", Nombre = "Patuca" }
                    }
                },
                new DepDto {
                    Codigo = "16", Nombre = "Santa Bárbara", Cabecera = "Santa Bárbara",
                    Municipios = new List<MunDto> {
                        new MunDto { Codigo = "1601", Nombre = "Santa Bárbara" },
                        new MunDto { Codigo = "1602", Nombre = "Arada" },
                        new MunDto { Codigo = "1603", Nombre = "Atima" },
                        new MunDto { Codigo = "1604", Nombre = "Azacualpa" },
                        new MunDto { Codigo = "1605", Nombre = "Ceguaca" },
                        new MunDto { Codigo = "1606", Nombre = "San José de las Colinas" },
                        new MunDto { Codigo = "1607", Nombre = "Concepción del Norte" },
                        new MunDto { Codigo = "1608", Nombre = "Concepción del Sur" },
                        new MunDto { Codigo = "1609", Nombre = "Chinda" },
                        new MunDto { Codigo = "1610", Nombre = "El Níspero" },
                        new MunDto { Codigo = "1611", Nombre = "Gualala" },
                        new MunDto { Codigo = "1612", Nombre = "Ilama" },
                        new MunDto { Codigo = "1613", Nombre = "Macuelizo" },
                        new MunDto { Codigo = "1614", Nombre = "Naranjito" },
                        new MunDto { Codigo = "1615", Nombre = "Nuevo Celilac" },
                        new MunDto { Codigo = "1616", Nombre = "Petoa" },
                        new MunDto { Codigo = "1617", Nombre = "Protección" },
                        new MunDto { Codigo = "1618", Nombre = "Quimistán" },
                        new MunDto { Codigo = "1619", Nombre = "San Francisco de Ojuera" },
                        new MunDto { Codigo = "1620", Nombre = "San Luis" },
                        new MunDto { Codigo = "1621", Nombre = "San Marcos" },
                        new MunDto { Codigo = "1622", Nombre = "San Nicolás" },
                        new MunDto { Codigo = "1623", Nombre = "San Pedro Zacapa" },
                        new MunDto { Codigo = "1624", Nombre = "Santa Rita" },
                        new MunDto { Codigo = "1625", Nombre = "Trinidad" },
                        new MunDto { Codigo = "1626", Nombre = "Las Vegas" },
                        new MunDto { Codigo = "1627", Nombre = "Nueva Frontera" }
                    }
                },
                new DepDto {
                    Codigo = "17", Nombre = "Valle", Cabecera = "Nacaome",
                    Municipios = new List<MunDto> {
                        new MunDto { Codigo = "1701", Nombre = "Nacaome" },
                        new MunDto { Codigo = "1702", Nombre = "Alianza" },
                        new MunDto { Codigo = "1703", Nombre = "Amapala" },
                        new MunDto { Codigo = "1704", Nombre = "Aramecina" },
                        new MunDto { Codigo = "1705", Nombre = "Caridad" },
                        new MunDto { Codigo = "1706", Nombre = "Goascorán" },
                        new MunDto { Codigo = "1707", Nombre = "Langue" },
                        new MunDto { Codigo = "1708", Nombre = "San Francisco de Coray" },
                        new MunDto { Codigo = "1709", Nombre = "San Lorenzo" }
                    }
                },
                new DepDto {
                    Codigo = "18", Nombre = "Yoro", Cabecera = "Yoro",
                    Municipios = new List<MunDto> {
                        new MunDto { Codigo = "1801", Nombre = "Yoro" },
                        new MunDto { Codigo = "1802", Nombre = "Arenal" },
                        new MunDto { Codigo = "1803", Nombre = "El Negrito" },
                        new MunDto { Codigo = "1804", Nombre = "El Progreso" },
                        new MunDto { Codigo = "1805", Nombre = "Jocón" },
                        new MunDto { Codigo = "1806", Nombre = "Morazán" },
                        new MunDto { Codigo = "1807", Nombre = "Olanchito" },
                        new MunDto { Codigo = "1808", Nombre = "Santa Rita" },
                        new MunDto { Codigo = "1809", Nombre = "Sulaco" },
                        new MunDto { Codigo = "1810", Nombre = "Victoria" },
                        new MunDto { Codigo = "1811", Nombre = "Yorito" }
                    }
                }
            };
        }
    }
}
